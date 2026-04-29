using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace LiPi.Clinic.Compliance.Entities;

public class LicenceType { public string Code { get; set; } = ""; public string Name { get; set; } = ""; public string IssuingAuthority { get; set; } = ""; public string DocumentClass { get; set; } = ""; public int? RenewalCycleMonths { get; set; } public List<string>? MandatoryForDepartments { get; set; } public string? ComplianceStandard { get; set; } public bool AuditRequired { get; set; } = true; public string? Description { get; set; } }

public class Licence { public Guid Id { get; set; } public Guid ClinicId { get; set; } public string LicenceTypeCode { get; set; } = ""; public Guid? DepartmentId { get; set; } public string LicenceNumber { get; set; } = ""; public DateTime IssuedDate { get; set; } public DateTime ValidFrom { get; set; } public DateTime? ValidTo { get; set; } public string? IssuedBy { get; set; } public string? AuthorityContact { get; set; } public string Status { get; set; } = "active"; public string? StatusReason { get; set; } public string? ConditionsText { get; set; } public string? PhysicalCopyS3Key { get; set; } public string? DigitalCopyUrl { get; set; } public DateTime? RenewalAppliedAt { get; set; } public DateTime? RenewalApprovedAt { get; set; } public Guid? PreviousLicenceId { get; set; } public bool IsCritical { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } public int RowVersion { get; set; } }

public class LicenceResponsibility { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid LicenceId { get; set; } public Guid StaffId { get; set; } public string ResponsibilityRole { get; set; } = ""; public DateTime AssignedAt { get; set; } public Guid? AssignedBy { get; set; } public DateTime ValidFrom { get; set; } public DateTime? ValidTo { get; set; } public string? Notes { get; set; } public DateTime CreatedAt { get; set; } public int RowVersion { get; set; } }

public class PhysicalCopyAccessLog { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid LicenceId { get; set; } public DateTime AccessedAt { get; set; } public Guid? ActorUserId { get; set; } public string ActorAction { get; set; } = ""; public string Purpose { get; set; } = ""; public string? LocationStoredAt { get; set; } public int? CustodyDurationMins { get; set; } public string? Notes { get; set; } }

public class RenewalSchedule { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid LicenceId { get; set; } public int DaysBeforeExpiry { get; set; } = 90; public DateTime DueDate { get; set; } public DateTime? RenewalSubmittedAt { get; set; } public DateTime? RenewalApprovedAt { get; set; } public Guid? NewLicenceId { get; set; } public string Status { get; set; } = "pending"; public Guid? AssignedTo { get; set; } public string? Notes { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } public int RowVersion { get; set; } }

public class ComplianceAlert { public Guid Id { get; set; } public Guid ClinicId { get; set; } public string AlertType { get; set; } = ""; public Guid? LicenceId { get; set; } public string Severity { get; set; } = ""; public DateTime RaisedAt { get; set; } public string TargetAudience { get; set; } = ""; public string Message { get; set; } = ""; public string? ActionRequired { get; set; } public DateTime? DueDate { get; set; } public string Status { get; set; } = "open"; public Guid? AcknowledgedBy { get; set; } public DateTime? AcknowledgedAt { get; set; } public DateTime? ResolvedAt { get; set; } public string? ResolutionNotes { get; set; } }

public class InspectionRecord { public Guid Id { get; set; } public Guid ClinicId { get; set; } public string InspectionType { get; set; } = ""; public DateTime InspectionDate { get; set; } public string ConductedBy { get; set; } = ""; public string? ScopeDescription { get; set; } public int? FindingsCount { get; set; } public int? CriticalFindings { get; set; } public int? MajorFindings { get; set; } public int? MinorFindings { get; set; } public bool CaraRequired { get; set; } public string? ReportS3Key { get; set; } public DateTime? ScheduledFollowupDate { get; set; } public DateTime? FollowupCompletedAt { get; set; } public string? Notes { get; set; } public DateTime CreatedAt { get; set; } public int RowVersion { get; set; } }

public class CaraItem { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid InspectionId { get; set; } public string? FindingCode { get; set; } public string Description { get; set; } = ""; public string? RootCauseAnalysis { get; set; } public string ActionPlanned { get; set; } = ""; public Guid? AssignedTo { get; set; } public DateTime DueDate { get; set; } public string Status { get; set; } = "open"; public DateTime? CompletionDate { get; set; } public DateTime? EffectivenessReviewDate { get; set; } public Guid? VerifiedBy { get; set; } public DateTime? VerifiedAt { get; set; } public string? Notes { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } public int RowVersion { get; set; } }

public class Policy { public Guid Id { get; set; } public Guid ClinicId { get; set; } public string PolicyCode { get; set; } = ""; public string PolicyTitle { get; set; } = ""; public string? Description { get; set; } public int Version { get; set; } public DateTime EffectiveFrom { get; set; } public DateTime? EffectiveTo { get; set; } public string? DocumentS3Key { get; set; } public DateTime? ApprovalDate { get; set; } public Guid? ApprovedBy { get; set; } public Guid? PublishedBy { get; set; } public List<string>? ComplianceMappedTo { get; set; } public int? ReviewCycleMonths { get; set; } public DateTime? NextReviewDue { get; set; } public string Status { get; set; } = "active"; public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } public int RowVersion { get; set; } }

public class TrainingCompletion { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid StaffId { get; set; } public string TrainingName { get; set; } = ""; public string TrainingCategory { get; set; } = ""; public DateTime TrainingDate { get; set; } public string? CertificateS3Key { get; set; } public decimal? CompletionHours { get; set; } public decimal? Score { get; set; } public bool Passed { get; set; } = true; public DateTime? ValidUntil { get; set; } public string? ConductedBy { get; set; } public string? Notes { get; set; } public DateTime CreatedAt { get; set; } public int RowVersion { get; set; } }

public class Incident { public Guid Id { get; set; } public Guid ClinicId { get; set; } public DateTime IncidentDate { get; set; } public string IncidentType { get; set; } = ""; public string Severity { get; set; } = ""; public Guid? ReportedBy { get; set; } public string Description { get; set; } = ""; public string? ImmediateAction { get; set; } public bool CaraRaised { get; set; } public Guid? CaraId { get; set; } public string Status { get; set; } = "reported"; public string? InvestigationNotes { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } public int RowVersion { get; set; } }

public class ComplianceDbContext : DbContext
{
    public ComplianceDbContext(DbContextOptions<ComplianceDbContext> options) : base(options) { }

    public DbSet<LicenceType> LicenceTypes => Set<LicenceType>();
    public DbSet<Licence> Licences => Set<Licence>();
    public DbSet<LicenceResponsibility> LicenceResponsibilities => Set<LicenceResponsibility>();
    public DbSet<PhysicalCopyAccessLog> PhysicalCopyAccessLogs => Set<PhysicalCopyAccessLog>();
    public DbSet<RenewalSchedule> RenewalSchedules => Set<RenewalSchedule>();
    public DbSet<ComplianceAlert> ComplianceAlerts => Set<ComplianceAlert>();
    public DbSet<InspectionRecord> InspectionRecords => Set<InspectionRecord>();
    public DbSet<CaraItem> CaraItems => Set<CaraItem>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<TrainingCompletion> TrainingCompletions => Set<TrainingCompletion>();
    public DbSet<Incident> Incidents => Set<Incident>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("compliance");

        foreach (var et in modelBuilder.Model.GetEntityTypes())
            foreach (var p in et.GetProperties())
                p.SetColumnName(ToSnakeCase(p.Name));

        modelBuilder.Entity<LicenceType>().HasKey(l => l.Code);
        modelBuilder.Entity<LicenceType>().Property(l => l.MandatoryForDepartments).HasColumnType("text[]");

        modelBuilder.Entity<Licence>().HasKey(l => l.Id);
        modelBuilder.Entity<Licence>().Property(l => l.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<Licence>().Property(l => l.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<Licence>().Property(l => l.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<Licence>().HasIndex(l => new { l.ClinicId, l.LicenceTypeCode, l.LicenceNumber }).IsUnique();
        modelBuilder.Entity<Licence>().HasIndex(l => l.DepartmentId).HasFilter("deleted_at IS NULL");
        modelBuilder.Entity<Licence>().Property(l => l.RowVersion).IsConcurrencyToken();

        modelBuilder.Entity<LicenceResponsibility>().HasKey(lr => lr.Id);
        modelBuilder.Entity<LicenceResponsibility>().Property(lr => lr.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<LicenceResponsibility>().Property(lr => lr.AssignedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<LicenceResponsibility>().Property(lr => lr.ValidFrom).HasDefaultValueSql("now()");
        modelBuilder.Entity<LicenceResponsibility>().HasIndex(lr => new { lr.StaffId, lr.ClinicId });

        modelBuilder.Entity<PhysicalCopyAccessLog>().HasKey(pcal => pcal.Id);
        modelBuilder.Entity<PhysicalCopyAccessLog>().Property(pcal => pcal.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<PhysicalCopyAccessLog>().Property(pcal => pcal.AccessedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<PhysicalCopyAccessLog>().HasIndex(pcal => new { pcal.LicenceId, pcal.AccessedAt }).IsDescending(false, true);

        modelBuilder.Entity<RenewalSchedule>().HasKey(rs => rs.Id);
        modelBuilder.Entity<RenewalSchedule>().Property(rs => rs.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<RenewalSchedule>().Property(rs => rs.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<RenewalSchedule>().Property(rs => rs.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<RenewalSchedule>().HasIndex(rs => new { rs.ClinicId, rs.DueDate }).HasFilter("status='pending'");

        modelBuilder.Entity<ComplianceAlert>().HasKey(ca => ca.Id);
        modelBuilder.Entity<ComplianceAlert>().Property(ca => ca.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<ComplianceAlert>().Property(ca => ca.RaisedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<ComplianceAlert>().HasIndex(ca => new { ca.ClinicId, ca.Severity, ca.RaisedAt }).IsDescending(false, false, true);

        modelBuilder.Entity<InspectionRecord>().HasKey(ir => ir.Id);
        modelBuilder.Entity<InspectionRecord>().Property(ir => ir.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<InspectionRecord>().HasIndex(ir => new { ir.ClinicId, ir.InspectionDate }).IsDescending(false, true);

        modelBuilder.Entity<CaraItem>().HasKey(ci => ci.Id);
        modelBuilder.Entity<CaraItem>().Property(ci => ci.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<CaraItem>().Property(ci => ci.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<CaraItem>().Property(ci => ci.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<CaraItem>().HasIndex(ci => new { ci.ClinicId, ci.Status, ci.DueDate }).HasFilter("status IN ('open','in_progress')");

        modelBuilder.Entity<Policy>().HasKey(p => p.Id);
        modelBuilder.Entity<Policy>().Property(p => p.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<Policy>().Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<Policy>().Property(p => p.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<Policy>().Property(p => p.ComplianceMappedTo).HasColumnType("text[]");
        modelBuilder.Entity<Policy>().HasIndex(p => new { p.ClinicId, p.PolicyCode, p.Version }).IsUnique();

        modelBuilder.Entity<TrainingCompletion>().HasKey(tc => tc.Id);
        modelBuilder.Entity<TrainingCompletion>().Property(tc => tc.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<TrainingCompletion>().Property(tc => tc.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<TrainingCompletion>().HasIndex(tc => new { tc.StaffId, tc.TrainingCategory, tc.TrainingDate }).IsDescending(false, false, true);

        modelBuilder.Entity<Incident>().HasKey(i => i.Id);
        modelBuilder.Entity<Incident>().Property(i => i.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<Incident>().Property(i => i.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<Incident>().Property(i => i.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<Incident>().HasIndex(i => new { i.ClinicId, i.IncidentDate }).IsDescending(false, true);
        modelBuilder.Entity<Incident>().Property(i => i.RowVersion).IsConcurrencyToken();
    }

    private static string ToSnakeCase(string input) { if (string.IsNullOrEmpty(input)) return input; var sb = new System.Text.StringBuilder(); for (int i = 0; i < input.Length; i++) { var c = input[i]; if (char.IsUpper(c)) { if (i > 0 && !char.IsUpper(input[i - 1])) sb.Append('_'); sb.Append(char.ToLowerInvariant(c)); } else sb.Append(c); } return sb.ToString(); }
}
