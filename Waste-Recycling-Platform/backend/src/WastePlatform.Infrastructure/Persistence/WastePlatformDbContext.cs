using Microsoft.EntityFrameworkCore;
using WastePlatform.Domain.Entities;

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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WastePlatformDbContext).Assembly);
    }
}

