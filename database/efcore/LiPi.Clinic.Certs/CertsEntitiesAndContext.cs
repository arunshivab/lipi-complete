using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace LiPi.Clinic.Certs.Entities;

public class CredentialType { public string Code { get; set; } = ""; public string CredentialName { get; set; } = ""; public string CredentialLevel { get; set; } = ""; public string Category { get; set; } = ""; public string IssuingAuthority { get; set; } = ""; public string IssuingCountry { get; set; } = ""; public int? ValidityYears { get; set; } public List<string>? MandatoryForRoles { get; set; } public bool MciRecognised { get; set; } public string? InternationalEquiv { get; set; } public string? Description { get; set; } }

public class StaffCredential { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid StaffId { get; set; } public string CredentialTypeCode { get; set; } = ""; public string CredentialNumber { get; set; } = ""; public DateTime IssuedDate { get; set; } public string? IssuedBy { get; set; } public DateTime ValidFrom { get; set; } public DateTime? ValidTo { get; set; } public string? CountryIssued { get; set; } public string? DisciplineSpecialisation { get; set; } public string? SubSpecialisation { get; set; } public string VerificationStatus { get; set; } = "unverified"; public string? VerificationMethod { get; set; } public DateTime? VerifiedAt { get; set; } public Guid? VerifiedBy { get; set; } public string? CertificateS3Key { get; set; } public string? PortalUrl { get; set; } public bool RenewalRequired { get; set; } public DateTime? RenewalDueDate { get; set; } public bool IsPrimary { get; set; } public string? ScopeLimitations { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } public int RowVersion { get; set; } }

public class BoardRegistration { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid StaffId { get; set; } public string BoardCode { get; set; } = ""; public string BoardName { get; set; } = ""; public string RegistrationNumber { get; set; } = ""; public string? RegistrationState { get; set; } public DateTime RegisteredDate { get; set; } public DateTime ValidFrom { get; set; } public DateTime? ValidTo { get; set; } public DateTime? RenewalDate { get; set; } public string RegistrationStatus { get; set; } = "active"; public string? SuspensionReason { get; set; } public DateTime? SuspensionFrom { get; set; } public DateTime? SuspensionTo { get; set; } public string? CertificateS3Key { get; set; } public string? OnlineVerifyUrl { get; set; } public bool IsPrimary { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } public int RowVersion { get; set; } }

public class CmeCredit { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid StaffId { get; set; } public string CreditType { get; set; } = ""; public string CreditProvider { get; set; } = ""; public decimal CreditPoints { get; set; } public DateTime CreditDate { get; set; } public string ActivityTitle { get; set; } = ""; public string? ActivityLocation { get; set; } public string? CertificateS3Key { get; set; } public string? CreditIdentifier { get; set; } public bool Approved { get; set; } public Guid? ApprovedBy { get; set; } public string? Notes { get; set; } public DateTime CreatedAt { get; set; } public int RowVersion { get; set; } }

public class CmeRequirement { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid StaffId { get; set; } public int Year { get; set; } public string RequirementType { get; set; } = ""; public decimal RequiredCredits { get; set; } public decimal EarnedCredits { get; set; } public DateTime DeadlineDate { get; set; } public string Status { get; set; } = "in_progress"; public string? ExemptionReason { get; set; } public Guid? ExemptedBy { get; set; } public DateTime? ExemptedAt { get; set; } public string? Comments { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } public int RowVersion { get; set; } }

public class CompetencyAssessment { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid StaffId { get; set; } public DateTime AssessmentDate { get; set; } public string AssessmentType { get; set; } = ""; public Guid AssessorId { get; set; } public string CompetencyLevel { get; set; } = ""; public decimal? Score { get; set; } public decimal? MaxScore { get; set; } public string? Comments { get; set; } public List<string>? EvidenceDocuments { get; set; } public string? ActionPlan { get; set; } public DateTime? FollowupDate { get; set; } public DateTime CreatedAt { get; set; } public int RowVersion { get; set; } }

public class TrainingRecord { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid StaffId { get; set; } public string TrainingCode { get; set; } = ""; public string TrainingName { get; set; } = ""; public string TrainingCategory { get; set; } = ""; public DateTime StartDate { get; set; } public DateTime? EndDate { get; set; } public decimal? DurationHours { get; set; } public string? TrainerName { get; set; } public string? TrainerOrg { get; set; } public string TrainingMode { get; set; } = ""; public string CompletionStatus { get; set; } = "in_progress"; public decimal? TestScore { get; set; } public bool TestRequired { get; set; } public bool? TestPassed { get; set; } public string? CertificateS3Key { get; set; } public bool CompetencyVerified { get; set; } public Guid? VerifiedBy { get; set; } public string? Notes { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } public int RowVersion { get; set; } }

public class CredentialHold { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid StaffId { get; set; } public string HoldReason { get; set; } = ""; public DateTime HeldFrom { get; set; } public DateTime? HeldUntil { get; set; } public string? HoldDetails { get; set; } public Guid? ImposedBy { get; set; } public bool AppealFiled { get; set; } public DateTime? AppealDate { get; set; } public string? AppealOutcome { get; set; } public DateTime? LiftedAt { get; set; } public Guid? LiftedBy { get; set; } public string? Notes { get; set; } public DateTime CreatedAt { get; set; } public int RowVersion { get; set; } }

public class Publication { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid StaffId { get; set; } public DateTime PublicationDate { get; set; } public string PublicationType { get; set; } = ""; public string? JournalName { get; set; } public string? ConferenceName { get; set; } public string Title { get; set; } = ""; public string? Authors { get; set; } public int? AuthorPosition { get; set; } public string? Doi { get; set; } public string? Pmid { get; set; } public string? Url { get; set; } public decimal? ImpactFactor { get; set; } public List<string>? Keywords { get; set; } public string? Abstract { get; set; } public bool IsCorrespondingAuthor { get; set; } public int? CitationCount { get; set; } public DateTime CreatedAt { get; set; } public int RowVersion { get; set; } }

public class Award { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid StaffId { get; set; } public DateTime AwardDate { get; set; } public string AwardName { get; set; } = ""; public string AwardingOrganization { get; set; } = ""; public string? AwardCategory { get; set; } public string? AwardLevel { get; set; } public string? CertificateS3Key { get; set; } public string? Notes { get; set; } public DateTime CreatedAt { get; set; } public int RowVersion { get; set; } }

public class CredentialVerificationChecklist { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid StaffId { get; set; } public DateTime ChecklistDate { get; set; } public bool MedicalDegreeVerified { get; set; } public bool BoardRegistrationVerified { get; set; } public bool LiabilityInsuranceVerified { get; set; } public bool BackgroundCheckVerified { get; set; } public bool ReferencesVerified { get; set; } public bool PreviousEmploymentVerified { get; set; } public bool DisciplinaryHistoryChecked { get; set; } public bool AllVerified { get; set; } public Guid? VerifiedBy { get; set; } public string? Notes { get; set; } public DateTime CreatedAt { get; set; } public int RowVersion { get; set; } }

public class CertsDbContext : DbContext
{
    public CertsDbContext(DbContextOptions<CertsDbContext> options) : base(options) { }

    public DbSet<CredentialType> CredentialTypes => Set<CredentialType>();
    public DbSet<StaffCredential> StaffCredentials => Set<StaffCredential>();
    public DbSet<BoardRegistration> BoardRegistrations => Set<BoardRegistration>();
    public DbSet<CmeCredit> CmeCredits => Set<CmeCredit>();
    public DbSet<CmeRequirement> CmeRequirements => Set<CmeRequirement>();
    public DbSet<CompetencyAssessment> CompetencyAssessments => Set<CompetencyAssessment>();
    public DbSet<TrainingRecord> TrainingRecords => Set<TrainingRecord>();
    public DbSet<CredentialHold> CredentialHolds => Set<CredentialHold>();
    public DbSet<Publication> Publications => Set<Publication>();
    public DbSet<Award> Awards => Set<Award>();
    public DbSet<CredentialVerificationChecklist> CredentialVerificationChecklists => Set<CredentialVerificationChecklist>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("certs");

        foreach (var et in modelBuilder.Model.GetEntityTypes())
            foreach (var p in et.GetProperties())
                p.SetColumnName(ToSnakeCase(p.Name));

        modelBuilder.Entity<CredentialType>().HasKey(c => c.Code);
        modelBuilder.Entity<CredentialType>().Property(c => c.MandatoryForRoles).HasColumnType("text[]");

        modelBuilder.Entity<StaffCredential>().HasKey(sc => sc.Id);
        modelBuilder.Entity<StaffCredential>().Property(sc => sc.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<StaffCredential>().Property(sc => sc.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<StaffCredential>().Property(sc => sc.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<StaffCredential>().HasIndex(sc => new { sc.ClinicId, sc.StaffId, sc.CredentialTypeCode }).IsUnique();
        modelBuilder.Entity<StaffCredential>().HasIndex(sc => new { sc.ClinicId, sc.RenewalDueDate }).HasFilter("renewal_required");
        modelBuilder.Entity<StaffCredential>().Property(sc => sc.RowVersion).IsConcurrencyToken();

        modelBuilder.Entity<BoardRegistration>().HasKey(br => br.Id);
        modelBuilder.Entity<BoardRegistration>().Property(br => br.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<BoardRegistration>().Property(br => br.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<BoardRegistration>().Property(br => br.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<BoardRegistration>().HasIndex(br => new { br.ClinicId, br.StaffId, br.BoardCode }).IsUnique();
        modelBuilder.Entity<BoardRegistration>().HasIndex(br => new { br.ClinicId, br.RenewalDate }).HasFilter("registration_status='active'");
        modelBuilder.Entity<BoardRegistration>().Property(br => br.RowVersion).IsConcurrencyToken();

        modelBuilder.Entity<CmeCredit>().HasKey(cc => cc.Id);
        modelBuilder.Entity<CmeCredit>().Property(cc => cc.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<CmeCredit>().Property(cc => cc.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<CmeCredit>().HasIndex(cc => new { cc.StaffId, cc.CreditDate }).IsDescending(false, true);

        modelBuilder.Entity<CmeRequirement>().HasKey(cr => cr.Id);
        modelBuilder.Entity<CmeRequirement>().Property(cr => cr.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<CmeRequirement>().Property(cr => cr.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<CmeRequirement>().Property(cr => cr.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<CmeRequirement>().HasIndex(cr => new { cr.ClinicId, cr.StaffId, cr.Year, cr.RequirementType }).IsUnique();
        modelBuilder.Entity<CmeRequirement>().Property(cr => cr.RowVersion).IsConcurrencyToken();

        modelBuilder.Entity<CompetencyAssessment>().HasKey(ca => ca.Id);
        modelBuilder.Entity<CompetencyAssessment>().Property(ca => ca.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<CompetencyAssessment>().Property(ca => ca.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<CompetencyAssessment>().Property(ca => ca.EvidenceDocuments).HasColumnType("text[]");
        modelBuilder.Entity<CompetencyAssessment>().HasIndex(ca => new { ca.StaffId, ca.AssessmentDate }).IsDescending(false, true);

        modelBuilder.Entity<TrainingRecord>().HasKey(tr => tr.Id);
        modelBuilder.Entity<TrainingRecord>().Property(tr => tr.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<TrainingRecord>().Property(tr => tr.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<TrainingRecord>().Property(tr => tr.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<TrainingRecord>().HasIndex(tr => new { tr.StaffId, tr.TrainingCategory, tr.EndDate }).IsDescending(false, false, true);
        modelBuilder.Entity<TrainingRecord>().Property(tr => tr.RowVersion).IsConcurrencyToken();

        modelBuilder.Entity<CredentialHold>().HasKey(ch => ch.Id);
        modelBuilder.Entity<CredentialHold>().Property(ch => ch.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<CredentialHold>().Property(ch => ch.HeldFrom).HasDefaultValueSql("now()");
        modelBuilder.Entity<CredentialHold>().HasIndex(ch => new { ch.StaffId, ch.HeldFrom }).IsDescending(false, true);

        modelBuilder.Entity<Publication>().HasKey(p => p.Id);
        modelBuilder.Entity<Publication>().Property(p => p.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<Publication>().Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<Publication>().Property(p => p.Keywords).HasColumnType("text[]");
        modelBuilder.Entity<Publication>().HasIndex(p => new { p.StaffId, p.PublicationDate }).IsDescending(false, true);

        modelBuilder.Entity<Award>().HasKey(a => a.Id);
        modelBuilder.Entity<Award>().Property(a => a.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<Award>().Property(a => a.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<Award>().HasIndex(a => new { a.StaffId, a.AwardDate }).IsDescending(false, true);

        modelBuilder.Entity<CredentialVerificationChecklist>().HasKey(cvc => cvc.Id);
        modelBuilder.Entity<CredentialVerificationChecklist>().Property(cvc => cvc.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<CredentialVerificationChecklist>().HasIndex(cvc => new { cvc.StaffId, cvc.ChecklistDate }).IsDescending(false, true);
    }

    private static string ToSnakeCase(string input) { if (string.IsNullOrEmpty(input)) return input; var sb = new System.Text.StringBuilder(); for (int i = 0; i < input.Length; i++) { var c = input[i]; if (char.IsUpper(c)) { if (i > 0 && !char.IsUpper(input[i - 1])) sb.Append('_'); sb.Append(char.ToLowerInvariant(c)); } else sb.Append(c); } return sb.ToString(); }
}
