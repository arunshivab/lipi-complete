using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace LiPi.Clinic.Security.Entities;

// Entities
public class EncryptionKey { public Guid Id { get; set; } public Guid ClinicId { get; set; } public string KeyPurpose { get; set; } = ""; public int KeyVersion { get; set; } public byte[] WrappedDek { get; set; } = Array.Empty<byte>(); public string KmsCmkArn { get; set; } = ""; public string Algorithm { get; set; } = "AES-256-GCM"; public bool IsActive { get; set; } = true; public DateTime ActivatedAt { get; set; } public DateTime? RetiredAt { get; set; } public DateTime RotationDueAt { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } public int RowVersion { get; set; } }

public class DataClassification { public string Code { get; set; } = ""; public string Name { get; set; } = ""; public string? Description { get; set; } public string? DpdpCategory { get; set; } public int? RetentionDays { get; set; } public bool EncryptionRequired { get; set; } = true; public bool AuditRequired { get; set; } = true; }

public class DpdpConsent { public Guid Id { get; set; } public Guid ClinicId { get; set; } public string DataPrincipalType { get; set; } = ""; public Guid DataPrincipalId { get; set; } public string PurposeCode { get; set; } = ""; public string PurposeText { get; set; } = ""; public string LawfulBasis { get; set; } = ""; public List<string> DataCategories { get; set; } = new(); public bool Granted { get; set; } public DateTime GrantedAt { get; set; } public DateTime ValidFrom { get; set; } public DateTime? ValidTo { get; set; } public string? WithdrawalChannel { get; set; } public DateTime? WithdrawnAt { get; set; } public string LanguageUsed { get; set; } = "en"; public string? ProofS3Key { get; set; } public string NoticeVersion { get; set; } = ""; public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } public int RowVersion { get; set; } }

public class DpdpSubjectRequest { public Guid Id { get; set; } public Guid ClinicId { get; set; } public string RequestType { get; set; } = ""; public string DataPrincipalType { get; set; } = ""; public Guid DataPrincipalId { get; set; } public string RequestChannel { get; set; } = ""; public DateTime RequestedAt { get; set; } public DateTime SlaDeadline { get; set; } public string Status { get; set; } = "received"; public Guid? AssignedTo { get; set; } public string? IdentityVerificationMethod { get; set; } public DateTime? IdentityVerifiedAt { get; set; } public DateTime? FulfilledAt { get; set; } public string? ResponseArtefactS3Key { get; set; } public string? RejectionReason { get; set; } public string? DpoNotes { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } public int RowVersion { get; set; } }

public class DpdpErasureTask { public Guid Id { get; set; } public Guid DpdpSubjectRequestId { get; set; } public string TargetSystem { get; set; } = ""; public string Status { get; set; } = "pending"; public long? RowsAffected { get; set; } public DateTime? ExecutedAt { get; set; } public string? ErrorDetail { get; set; } public string? LegalHoldReference { get; set; } }

public class SecurityEvent { public Guid Id { get; set; } public Guid ClinicId { get; set; } public DateTime OccurredAt { get; set; } public string EventCategory { get; set; } = ""; public string Severity { get; set; } = ""; public Guid? ActorUserId { get; set; } public System.Net.IPAddress? ActorIp { get; set; } public string? SourceHost { get; set; } public string? Signature { get; set; } public Dictionary<string, object> Detail { get; set; } = new(); public string Status { get; set; } = "open"; public Guid? TriagedBy { get; set; } public DateTime? TriagedAt { get; set; } public string? ResolutionNotes { get; set; } }

public class BreakGlassSession { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid UserId { get; set; } public Guid PatientId { get; set; } public DateTime ActivatedAt { get; set; } public DateTime ExpiresAt { get; set; } public string ReasonCode { get; set; } = ""; public string Justification { get; set; } = ""; public Guid? ApprovedBy { get; set; } public DateTime? ApprovalAt { get; set; } public Guid? ReviewedBy { get; set; } public DateTime? ReviewedAt { get; set; } public string? ReviewOutcome { get; set; } }

public class IpAllowlist { public Guid Id { get; set; } public Guid ClinicId { get; set; } public string Cidr { get; set; } = ""; public string Scope { get; set; } = ""; public string? Description { get; set; } public bool IsActive { get; set; } = true; public DateTime CreatedAt { get; set; } public Guid? CreatedBy { get; set; } public DateTime? ExpiresAt { get; set; } }

public class RateLimitBucket { public Guid Id { get; set; } public Guid ClinicId { get; set; } public string BucketKey { get; set; } = ""; public DateTime WindowStart { get; set; } public int RequestCount { get; set; } public int RejectedCount { get; set; } }

public class DataLocalizationPolicy { public Guid Id { get; set; } public Guid ClinicId { get; set; } public string DataCategory { get; set; } = ""; public string StorageRegion { get; set; } = ""; public List<string> ReplicationRegions { get; set; } = new(); public bool CrossBorderAllowed { get; set; } public List<string> CrossBorderCountries { get; set; } = new(); public string PolicyVersion { get; set; } = ""; public DateTime EffectiveFrom { get; set; } public DateTime? EffectiveTo { get; set; } public Guid? ApprovedBy { get; set; } public DateTime? ApprovedAt { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } }

// DbContext
public class SecurityDbContext : DbContext
{
    public SecurityDbContext(DbContextOptions<SecurityDbContext> options) : base(options) { }

    public DbSet<EncryptionKey> EncryptionKeys => Set<EncryptionKey>();
    public DbSet<DataClassification> DataClassifications => Set<DataClassification>();
    public DbSet<DpdpConsent> DpdpConsents => Set<DpdpConsent>();
    public DbSet<DpdpSubjectRequest> DpdpSubjectRequests => Set<DpdpSubjectRequest>();
    public DbSet<DpdpErasureTask> DpdpErasureTasks => Set<DpdpErasureTask>();
    public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();
    public DbSet<BreakGlassSession> BreakGlassSessions => Set<BreakGlassSession>();
    public DbSet<IpAllowlist> IpAllowlists => Set<IpAllowlist>();
    public DbSet<RateLimitBucket> RateLimitBuckets => Set<RateLimitBucket>();
    public DbSet<DataLocalizationPolicy> DataLocalizationPolicies => Set<DataLocalizationPolicy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("security");

        foreach (var et in modelBuilder.Model.GetEntityTypes())
            foreach (var p in et.GetProperties())
                p.SetColumnName(ToSnakeCase(p.Name));

        modelBuilder.Entity<EncryptionKey>().HasKey(e => e.Id);
        modelBuilder.Entity<EncryptionKey>().Property(e => e.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<EncryptionKey>().Property(e => e.ActivatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<EncryptionKey>().Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<EncryptionKey>().Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<EncryptionKey>().HasIndex(e => new { e.ClinicId, e.KeyPurpose }).HasFilter("is_active");
        modelBuilder.Entity<EncryptionKey>().Property(e => e.RowVersion).IsConcurrencyToken();

        modelBuilder.Entity<DataClassification>().HasKey(d => d.Code);

        modelBuilder.Entity<DpdpConsent>().HasKey(d => d.Id);
        modelBuilder.Entity<DpdpConsent>().Property(d => d.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<DpdpConsent>().Property(d => d.GrantedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<DpdpConsent>().Property(d => d.ValidFrom).HasDefaultValueSql("now()");
        modelBuilder.Entity<DpdpConsent>().Property(d => d.DataCategories).HasColumnType("text[]");
        modelBuilder.Entity<DpdpConsent>().HasIndex(d => new { d.DataPrincipalType, d.DataPrincipalId });
        modelBuilder.Entity<DpdpConsent>().Property(d => d.RowVersion).IsConcurrencyToken();

        modelBuilder.Entity<DpdpSubjectRequest>().HasKey(d => d.Id);
        modelBuilder.Entity<DpdpSubjectRequest>().Property(d => d.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<DpdpSubjectRequest>().Property(d => d.RequestedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<DpdpSubjectRequest>().HasIndex(d => new { d.ClinicId, d.Status, d.SlaDeadline });
        modelBuilder.Entity<DpdpSubjectRequest>().Property(d => d.RowVersion).IsConcurrencyToken();

        modelBuilder.Entity<DpdpErasureTask>().HasKey(d => d.Id);
        modelBuilder.Entity<DpdpErasureTask>().Property(d => d.Id).HasDefaultValueSql("core.uuid_v7()");

        modelBuilder.Entity<SecurityEvent>().HasKey(s => s.Id);
        modelBuilder.Entity<SecurityEvent>().Property(s => s.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<SecurityEvent>().Property(s => s.OccurredAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<SecurityEvent>().Property(s => s.Detail).HasColumnType("jsonb");
        modelBuilder.Entity<SecurityEvent>().Property(s => s.ActorIp).HasConversion(v => v != null ? v.ToString() : null, v => v != null ? System.Net.IPAddress.Parse(v) : null);
        modelBuilder.Entity<SecurityEvent>().HasIndex(s => new { s.ClinicId, s.Severity, s.OccurredAt }).IsDescending(false, false, true);

        modelBuilder.Entity<BreakGlassSession>().HasKey(b => b.Id);
        modelBuilder.Entity<BreakGlassSession>().Property(b => b.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<BreakGlassSession>().Property(b => b.ActivatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<BreakGlassSession>().HasIndex(b => new { b.PatientId, b.ActivatedAt }).IsDescending(false, true);

        modelBuilder.Entity<IpAllowlist>().HasKey(i => i.Id);
        modelBuilder.Entity<IpAllowlist>().Property(i => i.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<IpAllowlist>().Property(i => i.CreatedAt).HasDefaultValueSql("now()");

        modelBuilder.Entity<RateLimitBucket>().HasKey(r => r.Id);
        modelBuilder.Entity<RateLimitBucket>().Property(r => r.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<RateLimitBucket>().HasIndex(r => new { r.BucketKey, r.WindowStart }).IsUnique();

        modelBuilder.Entity<DataLocalizationPolicy>().HasKey(d => d.Id);
        modelBuilder.Entity<DataLocalizationPolicy>().Property(d => d.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<DataLocalizationPolicy>().Property(d => d.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<DataLocalizationPolicy>().Property(d => d.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<DataLocalizationPolicy>().Property(d => d.ReplicationRegions).HasColumnType("text[]");
        modelBuilder.Entity<DataLocalizationPolicy>().Property(d => d.CrossBorderCountries).HasColumnType("character(2)[]");
    }

    private static string ToSnakeCase(string input) { if (string.IsNullOrEmpty(input)) return input; var sb = new System.Text.StringBuilder(); for (int i = 0; i < input.Length; i++) { var c = input[i]; if (char.IsUpper(c)) { if (i > 0 && !char.IsUpper(input[i - 1])) sb.Append('_'); sb.Append(char.ToLowerInvariant(c)); } else sb.Append(c); } return sb.ToString(); }
}
