using Microsoft.EntityFrameworkCore;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace WastePlatform.Infrastructure.Persistence;

public class WastePlatformDbContext : DbContext
{
    public WastePlatformDbContext(DbContextOptions<WastePlatformDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Enterprise> Enterprises { get; set; } = null!;
    public DbSet<WasteCategory> WasteCategories { get; set; } = null!;
    public DbSet<Collector> Collectors { get; set; } = null!;
    public DbSet<EnterpriseWasteType> EnterpriseWasteTypes { get; set; } = null!;
    public DbSet<WasteReport> WasteReports { get; set; } = null!;
    public DbSet<ReportImage> ReportImages { get; set; } = null!;
    public DbSet<CollectionTask> CollectionTasks { get; set; } = null!;
    public DbSet<TaskStatusLog> TaskStatusLogs { get; set; } = null!;
    public DbSet<CollectionImage> CollectionImages { get; set; } = null!;
    public DbSet<RewardRule> RewardRules { get; set; } = null!;
    public DbSet<RewardPoints> RewardPoints { get; set; } = null!;
    public DbSet<Complaint> Complaints { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired().HasMaxLength(255);
            entity.Property(e => e.FullName).HasColumnName("full_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(15);
            entity.Property(e => e.District).HasColumnName("district").HasMaxLength(100);
            entity.Property(e => e.Ward).HasColumnName("ward").HasMaxLength(100);
            entity.Property(e => e.Role).HasColumnName("role").HasConversion<string>();
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired(false);

            entity.HasIndex(e => e.Email).IsUnique();
            // Phone nullable: filter index to avoid UNIQUE violation on multiple NULLs in MySQL
            entity.HasIndex(e => e.Phone).IsUnique().HasFilter("`Phone` IS NOT NULL");
            entity.HasIndex(e => new { e.District, e.Ward });
            entity.HasIndex(e => e.Role);
        });

        // Configure Enterprises
        modelBuilder.Entity<Enterprise>(entity =>
        {
            entity.ToTable("enterprises");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CompanyName).HasColumnName("company_name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.ServiceArea).HasColumnName("service_area").HasMaxLength(500);
            entity.Property(e => e.CapacityKgPerDay).HasColumnName("capacity_kg_per_day");
            entity.Property(e => e.IsVerified).HasColumnName("is_verified");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            
            entity.HasOne(e => e.User)
                .WithOne(u => u.Enterprise)
                .HasForeignKey<Enterprise>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Configure WasteCategories
        modelBuilder.Entity<WasteCategory>(entity =>
        {
            entity.ToTable("waste_categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure Collectors
        modelBuilder.Entity<Collector>(entity =>
        {
            entity.ToTable("collectors");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.EnterpriseId).HasColumnName("enterprise_id");
            entity.Property(e => e.IsAvailable).HasColumnName("is_available");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            
            entity.HasOne(e => e.User)
                .WithOne(u => u.Collector)
                .HasForeignKey<Collector>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Enterprise)
                .WithMany(en => en.Collectors)
                .HasForeignKey(e => e.EnterpriseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Configure EnterpriseWasteTypes
        modelBuilder.Entity<EnterpriseWasteType>(entity =>
        {
            entity.ToTable("enterprise_waste_types");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EnterpriseId).HasColumnName("enterprise_id");
            entity.Property(e => e.WasteCategoryId).HasColumnName("waste_category_id");
            
            entity.HasOne(e => e.Enterprise)
                .WithMany(en => en.WasteTypes)
                .HasForeignKey(e => e.EnterpriseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.WasteCategory)
                .WithMany(wc => wc.EnterpriseWasteTypes)
                .HasForeignKey(e => e.WasteCategoryId);

            entity.HasIndex(e => new { e.EnterpriseId, e.WasteCategoryId }).IsUnique();
        });

        // Configure WasteReports
        modelBuilder.Entity<WasteReport>(entity =>
        {
            entity.ToTable("waste_reports");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CitizenId).HasColumnName("citizen_id");
            entity.Property(e => e.WasteCategoryId).HasColumnName("waste_category_id");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(e => e.Latitude).HasColumnName("latitude").HasPrecision(10, 8);
            entity.Property(e => e.Longitude).HasColumnName("longitude").HasPrecision(11, 8);
            entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(500);
            entity.Property(e => e.AiSuggestion).HasColumnName("ai_suggestion").HasMaxLength(50);
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            
            entity.HasOne(e => e.Citizen)
                .WithMany(u => u.WasteReports)
                .HasForeignKey(e => e.CitizenId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.WasteCategory)
                .WithMany(wc => wc.WasteReports)
                .HasForeignKey(e => e.WasteCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.CitizenId);
            entity.HasIndex(e => e.WasteCategoryId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.Latitude, e.Longitude });
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure ReportImages
        modelBuilder.Entity<ReportImage>(entity =>
        {
            entity.ToTable("report_images");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url").IsRequired().HasMaxLength(500);
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.UploadedAt).HasColumnName("uploaded_at");
            
            entity.HasOne(e => e.WasteReport)
                .WithMany(r => r.Images)
                .HasForeignKey(e => e.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure CollectionTasks
        modelBuilder.Entity<CollectionTask>(entity =>
        {
            entity.ToTable("collection_tasks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.EnterpriseId).HasColumnName("enterprise_id");
            entity.Property(e => e.CollectorId).HasColumnName("collector_id");
            var collectionTaskStatusConverter = new ValueConverter<CollectionTaskStatus, string>(
                v => v == CollectionTaskStatus.OnTheWay ? "on_the_way" : v.ToString().ToLower(),
                v => v == "on_the_way" ? CollectionTaskStatus.OnTheWay : Enum.Parse<CollectionTaskStatus>(v, true)
            );

            entity.Property(e => e.Status).HasColumnName("status").HasConversion(collectionTaskStatusConverter);
            entity.Property(e => e.Notes).HasColumnName("notes").HasMaxLength(500);
            entity.Property(e => e.CollectedWeightKg).HasColumnName("collected_weight_kg").HasPrecision(8, 2);
            entity.Property(e => e.AssignedAt).HasColumnName("assigned_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            
            entity.HasOne(e => e.WasteReport)
                .WithOne(r => r.CollectionTask)
                .HasForeignKey<CollectionTask>(e => e.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Enterprise)
                .WithMany(en => en.CollectionTasks)
                .HasForeignKey(e => e.EnterpriseId);

            entity.HasOne(e => e.Collector)
                .WithMany(c => c.CollectionTasks)
                .HasForeignKey(e => e.CollectorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.ReportId).IsUnique();
            entity.HasIndex(e => e.EnterpriseId);
            entity.HasIndex(e => e.CollectorId);
            entity.HasIndex(e => e.Status);
        });

        // Configure TaskStatusLogs
        modelBuilder.Entity<TaskStatusLog>(entity =>
        {
            entity.ToTable("task_status_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            var logTaskStatusConverter = new ValueConverter<CollectionTaskStatus, string>(
                v => v == CollectionTaskStatus.OnTheWay ? "on_the_way" : v.ToString().ToLower(),
                v => v == "on_the_way" ? CollectionTaskStatus.OnTheWay : Enum.Parse<CollectionTaskStatus>(v, true)
            );

            entity.Property(e => e.Status).HasColumnName("status").HasConversion(logTaskStatusConverter);
            entity.Property(e => e.ChangedAt).HasColumnName("changed_at");
            
            entity.HasOne(e => e.CollectionTask)
                .WithMany(t => t.StatusLogs)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.TaskId);
        });

        // Configure CollectionImages
        modelBuilder.Entity<CollectionImage>(entity =>
        {
            entity.ToTable("collection_images");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url").IsRequired().HasMaxLength(500);
            
            entity.HasOne(e => e.CollectionTask)
                .WithMany(t => t.Images)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure RewardRules
        modelBuilder.Entity<RewardRule>(entity =>
        {
            entity.ToTable("reward_rules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EnterpriseId).HasColumnName("enterprise_id");
            entity.Property(e => e.WasteCategoryId).HasColumnName("waste_category_id");
            entity.Property(e => e.PointsPerReport).HasColumnName("points_per_report");
            entity.Property(e => e.BonusQuality).HasColumnName("bonus_quality");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            
            entity.HasOne(e => e.Enterprise)
                .WithMany(en => en.RewardRules)
                .HasForeignKey(e => e.EnterpriseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.WasteCategory)
                .WithMany(wc => wc.RewardRules)
                .HasForeignKey(e => e.WasteCategoryId);

            entity.HasIndex(e => new { e.EnterpriseId, e.WasteCategoryId }).IsUnique();
        });

        // Configure RewardPoints
        modelBuilder.Entity<RewardPoints>(entity =>
        {
            entity.ToTable("reward_points");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CitizenId).HasColumnName("citizen_id");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.Points).HasColumnName("points");
            entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(255);
            entity.Property(e => e.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            
            entity.HasOne(e => e.Citizen)
                .WithMany(u => u.RewardPoints)
                .HasForeignKey(e => e.CitizenId);

            entity.HasOne(e => e.WasteReport)
                .WithMany(r => r.RewardPoints)
                .HasForeignKey(e => e.ReportId)
                .OnDelete(DeleteBehavior.SetNull);

            // IdempotencyKey nullable: only unique when non-null
            entity.HasIndex(e => e.IdempotencyKey).IsUnique().HasFilter("`IdempotencyKey` IS NOT NULL");
            entity.HasIndex(e => new { e.CitizenId, e.CreatedAt });
        });

        // Configure Complaints
        modelBuilder.Entity<Complaint>(entity =>
        {
            entity.ToTable("complaints");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CitizenId).HasColumnName("citizen_id");
            entity.Property(e => e.EnterpriseId).HasColumnName("enterprise_id");
            entity.Property(e => e.CollectorId).HasColumnName("collector_id");
            entity.Property(e => e.ReportId).HasColumnName("report_id");
            entity.Property(e => e.Content).HasColumnName("content").HasMaxLength(2000);
            entity.Property(e => e.AdminResponse).HasColumnName("admin_response").HasMaxLength(2000);
            entity.Property(e => e.EnterpriseResponse).HasColumnName("enterprise_response").HasMaxLength(2000);
            entity.Property(e => e.EnterpriseRespondedAt).HasColumnName("enterprise_responded_at").IsRequired(false);
            entity.Property(e => e.EscalationReason).HasColumnName("escalation_reason").HasMaxLength(1000).IsRequired(false);
            // Convert ComplaintStatus enum to snake_case string for MySQL ENUM
            var complaintStatusConverter = new ValueConverter<ComplaintStatus, string>(
                v => v == ComplaintStatus.InProgress ? "in_progress" : v.ToString().ToLower(),
                v => v == "in_progress" ? ComplaintStatus.InProgress : Enum.Parse<ComplaintStatus>(v, true)
            );
            entity.Property(e => e.Status).HasColumnName("status").HasConversion(complaintStatusConverter);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at").IsRequired(false);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired(false);
            
            entity.HasOne(e => e.Citizen)
                .WithMany(u => u.Complaints)
                .HasForeignKey(e => e.CitizenId);

            entity.HasOne(e => e.Enterprise)
                .WithMany()
                .HasForeignKey(e => e.EnterpriseId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<Collector>()
                .WithMany()
                .HasForeignKey(e => e.CollectorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.WasteReport)
                .WithMany(r => r.Complaints)
                .HasForeignKey(e => e.ReportId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.CitizenId);
            entity.HasIndex(e => e.Status);
        });

        // Configure AuditLogs
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Action).HasColumnName("action").IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).HasColumnName("entity_type").HasMaxLength(50);
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
        });

        // Configure Notifications
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CitizenId).HasColumnName("citizen_id").IsRequired(false);
            entity.Property(e => e.Type).HasColumnName("type").HasConversion<string>();
            entity.Property(e => e.Channel).HasColumnName("channel").HasConversion<string>();
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).HasColumnName("message").IsRequired().HasMaxLength(1000);
            entity.Property(e => e.ActionUrl).HasColumnName("action_url").HasMaxLength(500);
            entity.Property(e => e.RelatedEntityId).HasColumnName("related_entity_id");
            entity.Property(e => e.RelatedEntityType).HasColumnName("related_entity_type").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ReadAt).HasColumnName("read_at").IsRequired(false);
            
            entity.HasOne(e => e.Citizen)
                .WithMany()
                .HasForeignKey(e => e.CitizenId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.CitizenId);
            entity.HasIndex(e => new { e.CitizenId, e.Status });
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
