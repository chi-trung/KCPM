using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Reflection; 
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Application.Services;
using WastePlatform.Infrastructure.Persistence;
using WastePlatform.Infrastructure.Services;
using WastePlatform.Infrastructure.SignalR;
// Thêm thư mục chứa UserRepository (điều chỉnh lại nếu bạn để thư mục khác nhé)
using WastePlatform.Infrastructure.Persistence.Repositories; 
using WastePlatform.API.Converters;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;

var builder = WebApplication.CreateBuilder(args);

// ── Database ────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Auto-convert MySQL URL format (mysql://user:pass@host:port/db) to ADO.NET format
// Cloud providers like Aiven provide URL format, but MySqlConnector needs ADO.NET format
if (connectionString.StartsWith("mysql://", StringComparison.OrdinalIgnoreCase))
{
    var uri = new Uri(connectionString);
    var userInfo = uri.UserInfo.Split(':');
    var user = Uri.UnescapeDataString(userInfo[0]);
    var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 3306;
    var database = uri.AbsolutePath.TrimStart('/');
    // Parse ssl-mode from query string
    var sslMode = "Required";
    if (!string.IsNullOrEmpty(uri.Query))
    {
        var queryParams = uri.Query.TrimStart('?').Split('&');
        foreach (var param in queryParams)
        {
            var kv = param.Split('=', 2);
            if (kv.Length == 2 && kv[0].Equals("ssl-mode", StringComparison.OrdinalIgnoreCase))
                sslMode = kv[1];
        }
    }
    connectionString = $"Server={host};Port={port};Database={database};Uid={user};Pwd={pass};SslMode={sslMode};ConnectionTimeout=30;DefaultCommandTimeout=60";
    Console.WriteLine($"✅ Converted MySQL URL to ADO.NET format (host={host}, db={database})");
}

builder.Services.AddDbContext<WastePlatformDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 30)))
);

// ── JWT Authentication ───────────────────────────────────────────────
var jwtSecret = builder.Configuration["JwtSettings:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience            = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };

        // Cho phép SignalR WebSockets nhận token từ Query String
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── Application Services ─────────────────────────────────────────────
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<AuthService>();

// 👉 ĐÃ THÊM: Đăng ký UserRepository để chọc xuống Database
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IEnterpriseRepository, EnterpriseRepository>();

// Repositories for Reports and Categories
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IWasteCategoryRepository, WasteCategoryRepository>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

// 👉 Repositories for Admin Module
builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

// 👉 Repositories for Citizen Module (Rewards & Complaints)
builder.Services.AddScoped<IRewardPointsRepository, RewardPointsRepository>();

// 👉 Notification System
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRealTimeNotifier, SignalRRealTimeNotifier>();

// Đăng ký MediatR để xử lý CQRS (Queries/Commands)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

// Đăng ký SignalR cho Real-time Updates (WRP-113)
builder.Services.AddSignalR();

// ── CORS ─────────────────────────────────────────────────────────────
var allowedOrigins = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "http://localhost:3000",
    "https://kcpm-ecru.vercel.app",
    "https://kcpm.vercel.app"
};
// Add configured frontend URLs (comma-separated) from environment
var frontendUrls = builder.Configuration["FrontendUrls"];
if (!string.IsNullOrEmpty(frontendUrls))
{
    foreach (var url in frontendUrls.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        allowedOrigins.Add(url);
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", corsBuilder =>
        corsBuilder
            .SetIsOriginAllowed(origin =>
            {
                // Allow explicit origins
                if (allowedOrigins.Contains(origin)) return true;
                // Allow all *.vercel.app subdomains (preview deployments)
                if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                    return uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase);
                return false;
            })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()); // Required for SignalR with authentication
});

// ── Controllers & Swagger ─────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize DateTime as UTC (ISO 8601 with 'Z' suffix)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new DateTimeUtcConverter());
        options.JsonSerializerOptions.Converters.Add(new DateTimeNullableUtcConverter());
        // Serialize enums as strings instead of integers
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
    {
        // Thêm mô tả cho JWT Authentication
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Nhập 'Bearer' theo sau là token JWT của bạn."
        });

        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
);

var app = builder.Build();

// ── Database auto-migration + seed ──────────────────────────────────────────
// EnsureCreated creates all tables from the EF Core model if they don't exist.
// Then we seed essential data (categories, sample accounts) if tables are empty.
// This is needed for cloud deployments (Render + Aiven MySQL) where Docker Compose
// is not available to mount SQL migration scripts.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WastePlatformDbContext>();
    try
    {
        db.Database.EnsureCreated();
        Console.WriteLine("✅ Database schema verified/created successfully.");

        // ── Auto-seed: Waste Categories ─────────────────────────────
        if (!db.WasteCategories.Any())
        {
            Console.WriteLine("🌱 Seeding waste categories...");
            db.Database.ExecuteSqlRaw(@"
                INSERT INTO waste_categories (id, name, description) VALUES
                (1, 'Rác thải sinh hoạt', 'Rác thải từ nhà ở, cơ quan, cửa hàng'),
                (2, 'Rác thải thực phẩm', 'Thực phẩm thừa, xương, rau quả'),
                (3, 'Rác thải nguy hiểm', 'Pin, thuốc, hóa chất, v.v.'),
                (4, 'Rác thải xây dựng', 'Xi măng, gạch, thép, v.v.'),
                (5, 'Rác thải cây lá', 'Lá rơi, cành cây, cỏ, v.v.')
            ");
            Console.WriteLine("✅ Seeded 5 waste categories.");
        }

        // ── Auto-seed: Sample user accounts ─────────────────────────
        // Check if seed accounts already exist (by looking for admin with Admin role)
        var adminExists = db.Users.Any(u => u.Email == "admin@gmail.com" && u.Role == UserRole.Admin);
        if (!adminExists)
        {
            Console.WriteLine("🌱 Seeding sample user accounts...");
            // Seed password: use SEED_PASSWORD env var or default
            // Note: This is dev/staging seed data only, NOT production credentials
            var seedPassword = Environment.GetEnvironmentVariable("SEED_PASSWORD") ?? "S33dP@ss!2026";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(seedPassword);

            // Clean up any existing seed emails that may have been registered with wrong roles
            var seedEmails = new[] { "admin@gmail.com", "nguyenvana@gmail.com", "lethib@gmail.com",
                "tranvanc@gmail.com", "greenlife@gmail.com", "ecofriendly@gmail.com",
                "collector1@gmail.com", "collector2@gmail.com" };
            var existingSeedUsers = db.Users.Where(u => seedEmails.Contains(u.Email)).ToList();
            if (existingSeedUsers.Any())
            {
                Console.WriteLine($"🧹 Removing {existingSeedUsers.Count} incorrectly registered seed accounts...");
                db.Users.RemoveRange(existingSeedUsers);
                db.SaveChanges();
            }

            // Admin
            var admin = User.Create("admin@gmail.com", passwordHash, "System Administrator", UserRole.Admin);

            // Citizens
            var citizen1 = User.Create("nguyenvana@gmail.com", passwordHash, "Nguyễn Văn A", UserRole.Citizen, "0901234561", "Quận 1", "Phường Bến Nghé");
            var citizen2 = User.Create("lethib@gmail.com", passwordHash, "Lê Thị B", UserRole.Citizen, "0901234562", "Quận 3", "Phường Võ Thị Sáu");
            var citizen3 = User.Create("tranvanc@gmail.com", passwordHash, "Trần Văn C", UserRole.Citizen, "0901234563", "Quận Bình Thạnh", "Phường 25");

            // Enterprises
            var enterprise1User = User.Create("greenlife@gmail.com", passwordHash, "Green Life CEO", UserRole.Enterprise, "0283800001");
            var enterprise2User = User.Create("ecofriendly@gmail.com", passwordHash, "EcoFriendly Manager", UserRole.Enterprise, "0283800002");

            // Collectors
            var collector1User = User.Create("collector1@gmail.com", passwordHash, "Phạm Minh Dũng", UserRole.Collector, "0911000001");
            var collector2User = User.Create("collector2@gmail.com", passwordHash, "Lý Đại Nghĩa", UserRole.Collector, "0911000002");

            db.Users.AddRange(admin, citizen1, citizen2, citizen3, enterprise1User, enterprise2User, collector1User, collector2User);
            db.SaveChanges();

            // Enterprise profiles
            var ent1 = new Enterprise
            {
                Id = Guid.NewGuid(),
                UserId = enterprise1User.Id,
                CompanyName = "Công ty Tái chế Green Life",
                CapacityKgPerDay = 5000,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            var ent2 = new Enterprise
            {
                Id = Guid.NewGuid(),
                UserId = enterprise2User.Id,
                CompanyName = "Eco-Friendly Collection",
                CapacityKgPerDay = 3500,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            db.Enterprises.Add(ent1);
            db.Enterprises.Add(ent2);
            db.SaveChanges();

            // Collector profiles
            db.Collectors.Add(new Collector
            {
                Id = Guid.NewGuid(),
                UserId = collector1User.Id,
                EnterpriseId = ent1.Id,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            });
            db.Collectors.Add(new Collector
            {
                Id = Guid.NewGuid(),
                UserId = collector2User.Id,
                EnterpriseId = ent2.Id,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();

            // Enterprise waste types
            db.EnterpriseWasteTypes.Add(new EnterpriseWasteType
            {
                Id = Guid.NewGuid(),
                EnterpriseId = ent1.Id,
                WasteCategoryId = 1
            });
            db.EnterpriseWasteTypes.Add(new EnterpriseWasteType
            {
                Id = Guid.NewGuid(),
                EnterpriseId = ent2.Id,
                WasteCategoryId = 2
            });
            db.SaveChanges();

            Console.WriteLine("✅ Seeded 8 user accounts (1 admin, 3 citizens, 2 enterprises, 2 collectors).");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Database initialization warning: {ex.Message}");
        // Don't crash the app — let it start and handle DB errors per-request
    }
}

// ── Middleware pipeline ───────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Enable Swagger in Production for debugging
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "swagger";
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Waste Platform API V1");
    });
}

// Explicitly configure static files for the uploads directory
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// NOTE: No UseHttpsRedirection() — Docker runs plain HTTP on port 8080
app.UseCors("AllowFrontend");
app.UseAuthentication();

app.UseMiddleware<WastePlatform.API.Middleware.ValidateUserStatusMiddleware>();

app.UseAuthorization();
app.MapControllers();

// Map SignalR Hub
app.MapHub<TaskHub>("/hubs/task");

app.Run();