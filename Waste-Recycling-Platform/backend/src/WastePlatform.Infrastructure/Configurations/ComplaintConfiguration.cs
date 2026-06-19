using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;

namespace WastePlatform.Infrastructure.Configurations;

/// <summary>
/// EF Core entity type configuration for the <see cref="Complaint"/> entity.
/// </summary>
public class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
{
    // Convert ComplaintStatus enum to snake_case string for MySQL ENUM
    private static readonly ValueConverter<ComplaintStatus, string> ComplaintStatusConverter = new(
        v => v == ComplaintStatus.InProgress ? "in_progress" : v.ToString().ToLower(),
        v => v == "in_progress" ? ComplaintStatus.InProgress : Enum.Parse<ComplaintStatus>(v, true)
    );

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Complaint> entity)
    {
        // Configure Complaints
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
        entity.Property(e => e.Status).HasColumnName("status").HasConversion(ComplaintStatusConverter);
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
    }
}
