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

var builder = WebApplication.CreateBuilder(args);

// ── Database ────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<WastePlatformDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
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
var allowedOrigins = new List<string> { "http://localhost:3000" };
// Add configured frontend URLs (comma-separated) from environment
var frontendUrls = builder.Configuration["FrontendUrls"];
if (!string.IsNullOrEmpty(frontendUrls))
{
    allowedOrigins.AddRange(frontendUrls.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
// Always allow the Vercel production domain
allowedOrigins.Add("https://kcpm-ecru.vercel.app");
allowedOrigins.Add("https://kcpm.vercel.app");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", corsBuilder =>
        corsBuilder
            .WithOrigins(allowedOrigins.ToArray())
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

// ── Database is initialized via SQL migration scripts in docker-compose ───
// The db/migrations folder is mounted to /docker-entrypoint-initdb.d in MySQL
// Auto-migration is skipped since DDL is managed by versioned SQL files

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