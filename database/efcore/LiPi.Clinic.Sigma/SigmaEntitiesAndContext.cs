using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace LiPi.Clinic.Sigma.Entities;

public class ProcessDefinition { public Guid Id { get; set; } public Guid ClinicId { get; set; } public string ProcessCode { get; set; } = ""; public string ProcessName { get; set; } = ""; public int ProcessLevel { get; set; } public Guid? ParentProcessId { get; set; } public Guid? DepartmentId { get; set; } public Guid ProcessOwnerId { get; set; } public string? ProcessObjective { get; set; } public string? KpiCode { get; set; } public decimal? KpiTarget { get; set; } public string? KpiUnit { get; set; } public decimal? UpperSpecLimit { get; set; } public decimal? LowerSpecLimit { get; set; } public decimal? TargetValue { get; set; } public bool CriticalStep { get; set; } public string? DocumentationS3Key { get; set; } public DateTime EffectiveFrom { get; set; } public DateTime? EffectiveTo { get; set; } public string Status { get; set; } = "active"; public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } public int RowVersion { get; set; } }

public class ProcessStep { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid ProcessId { get; set; } public int StepSequence { get; set; } public string StepName { get; set; } = ""; public string? StepDescription { get; set; } public string? ResponsibleRoleCode { get; set; } public int? EstimatedDurationMins { get; set; } public string? SuccessCriteria { get; set; } public string? RiskLevel { get; set; } public string? MitigatingControl { get; set; } public DateTime CreatedAt { get; set; } public int RowVersion { get; set; } }

public class Defect { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid ProcessId { get; set; } public DateTime DefectDate { get; set; } public string? DefectCode { get; set; } public string DefectType { get; set; } = ""; public string Severity { get; set; } = ""; public string Description { get; set; } = ""; public string? RootCause { get; set; } public Guid? DetectedBy { get; set; } public string? DetectedStage { get; set; } public string? ImmediateAction { get; set; } public bool CaraRequired { get; set; } public Guid? CaraId { get; set; } public decimal? DefectCost { get; set; } public string Status { get; set; } = "reported"; public DateTime? ClosedAt { get; set; } public bool TrendFlag { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } public int RowVersion { get; set; } }

public class SpcObservation { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid ProcessId { get; set; } public DateTime ObservationDate { get; set; } public TimeSpan? ObservationTime { get; set; } public string? ObservationBatch { get; set; } public decimal ObservationValue { get; set; } public string? ObservationUnit { get; set; } public Guid? SubgroupId { get; set; } public bool IsOutlier { get; set; } }

public class SpcRollup { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid ProcessId { get; set; } public DateTime RollupDate { get; set; } public string RollupPeriod { get; set; } = ""; public int SampleCount { get; set; } public decimal? MeanValue { get; set; } public decimal? StdDev { get; set; } public decimal? MinValue { get; set; } public decimal? MaxValue { get; set; } public decimal? MedianValue { get; set; } public decimal? Q1Value { get; set; } public decimal? Q3Value { get; set; } public decimal? IqrValue { get; set; } public decimal? CpIndex { get; set; } public decimal? CpkIndex { get; set; } public decimal? PpIndex { get; set; } public decimal? PpkIndex { get; set; } public decimal? ProcessYield { get; set; } public int? DefectsCount { get; set; } public bool OutOfControl { get; set; } public string? ControlRuleViolated { get; set; } public DateTime CreatedAt { get; set; } public int RowVersion { get; set; } }

public class ControlChartPoint { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid ProcessId { get; set; } public string ChartType { get; set; } = ""; public DateTime PointDate { get; set; } public decimal PointValue { get; set; } public decimal? Ucl { get; set; } public decimal? Lcl { get; set; } public decimal? CenterLine { get; set; } public decimal? Usl { get; set; } public decimal? Lsl { get; set; } public bool IsOutOfControl { get; set; } public string? SpcRuleViolated { get; set; } public string? AlertLevel { get; set; } public DateTime CreatedAt { get; set; } public int RowVersion { get; set; } }

public class FmeaItem { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid ProcessId { get; set; } public Guid? ProcessStepId { get; set; } public string FailureMode { get; set; } = ""; public string PotentialEffects { get; set; } = ""; public string? PotentialCauses { get; set; } public int SeverityScore { get; set; } public int OccurrenceScore { get; set; } public int DetectionScore { get; set; } public int RpnValue { get; set; } public string? CurrentControls { get; set; } public string? RecommendedAction { get; set; } public Guid? ActionOwner { get; set; } public DateTime? ActionDeadline { get; set; } public string? ActionTaken { get; set; } public bool? ActionEffective { get; set; } public int? RevisedSeverity { get; set; } public int? RevisedOccurrence { get; set; } public int? RevisedDetection { get; set; } public int? RevisedRpn { get; set; } public string? Notes { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } public int RowVersion { get; set; } }

public class Project { public Guid Id { get; set; } public Guid ClinicId { get; set; } public string ProjectCode { get; set; } = ""; public string ProjectName { get; set; } = ""; public string ProjectType { get; set; } = ""; public Guid? RelatedProcessId { get; set; } public Guid SponsorId { get; set; } public Guid ProjectLeadId { get; set; } public List<Guid>? TeamMembers { get; set; } public string? CharterS3Key { get; set; } public string? ProblemStatement { get; set; } public string? GoalStatement { get; set; } public string? Scope { get; set; } public decimal? BaselineMetric { get; set; } public decimal? TargetMetric { get; set; } public string? MetricUnit { get; set; } public DateTime ProjectStartDate { get; set; } public DateTime? TargetCompletionDate { get; set; } public DateTime? ActualCompletionDate { get; set; } public string Status { get; set; } = "initiated"; public bool PhaseDefineComplete { get; set; } public bool PhaseMeasureComplete { get; set; } public bool PhaseAnalyzeComplete { get; set; } public bool PhaseImproveComplete { get; set; } public bool PhaseControlComplete { get; set; } public decimal? FinancialBenefit { get; set; } public string? Notes { get; set; } public DateTime CreatedAt { get; set; } public DateTime UpdatedAt { get; set; } public int RowVersion { get; set; } }

public class KaizenSuggestion { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid ProcessId { get; set; } public Guid SuggestedBy { get; set; } public DateTime SuggestedDate { get; set; } public string SuggestionTitle { get; set; } = ""; public string SuggestionDetail { get; set; } = ""; public string? ExpectedBenefit { get; set; } public string? EstimatedEffort { get; set; } public string Status { get; set; } = "submitted"; public Guid? ReviewedBy { get; set; } public DateTime? ReviewedDate { get; set; } public string? ReviewComments { get; set; } public Guid? ImplementationOwner { get; set; } public DateTime? ImplementationDate { get; set; } public string? VerifiedBenefit { get; set; } public DateTime CreatedAt { get; set; } public int RowVersion { get; set; } }

public class ProcessTrendSummary { public Guid Id { get; set; } public Guid ClinicId { get; set; } public Guid ProcessId { get; set; } public DateTime SummaryDate { get; set; } public int? DefectCount30d { get; set; } public int? DefectCount90d { get; set; } public int? DefectCount365d { get; set; } public string? TrendDirection { get; set; } public decimal? CpkCurrent { get; set; } public string? CpkTrend { get; set; } public DateTime? LastOutOfControlDate { get; set; } public string? ProcessAlertLevel { get; set; } public string? Notes { get; set; } public DateTime UpdatedAt { get; set; } }

public class SigmaDbContext : DbContext
{
    public SigmaDbContext(DbContextOptions<SigmaDbContext> options) : base(options) { }

    public DbSet<ProcessDefinition> ProcessDefinitions => Set<ProcessDefinition>();
    public DbSet<ProcessStep> ProcessSteps => Set<ProcessStep>();
    public DbSet<Defect> Defects => Set<Defect>();
    public DbSet<SpcObservation> SpcObservations => Set<SpcObservation>();
    public DbSet<SpcRollup> SpcRollups => Set<SpcRollup>();
    public DbSet<ControlChartPoint> ControlChartPoints => Set<ControlChartPoint>();
    public DbSet<FmeaItem> FmeaItems => Set<FmeaItem>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<KaizenSuggestion> KaizenSuggestions => Set<KaizenSuggestion>();
    public DbSet<ProcessTrendSummary> ProcessTrendSummaries => Set<ProcessTrendSummary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("sigma");

        foreach (var et in modelBuilder.Model.GetEntityTypes())
            foreach (var p in et.GetProperties())
                p.SetColumnName(ToSnakeCase(p.Name));

        modelBuilder.Entity<ProcessDefinition>().HasKey(pd => pd.Id);
        modelBuilder.Entity<ProcessDefinition>().Property(pd => pd.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<ProcessDefinition>().Property(pd => pd.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<ProcessDefinition>().Property(pd => pd.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<ProcessDefinition>().Property(pd => pd.EffectiveFrom).HasDefaultValueSql("CURRENT_DATE");
        modelBuilder.Entity<ProcessDefinition>().HasIndex(pd => new { pd.ClinicId, pd.ProcessCode }).IsUnique();
        modelBuilder.Entity<ProcessDefinition>().HasIndex(pd => pd.DepartmentId).HasFilter("status='active'");
        modelBuilder.Entity<ProcessDefinition>().Property(pd => pd.RowVersion).IsConcurrencyToken();

        modelBuilder.Entity<ProcessStep>().HasKey(ps => ps.Id);
        modelBuilder.Entity<ProcessStep>().Property(ps => ps.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<ProcessStep>().Property(ps => ps.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<ProcessStep>().HasIndex(ps => ps.ProcessId);
        modelBuilder.Entity<ProcessStep>().HasIndex(ps => new { ps.ClinicId, ps.ProcessId, ps.StepSequence }).IsUnique();

        modelBuilder.Entity<Defect>().HasKey(d => d.Id);
        modelBuilder.Entity<Defect>().Property(d => d.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<Defect>().Property(d => d.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<Defect>().Property(d => d.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<Defect>().HasIndex(d => new { d.ProcessId, d.DefectDate }).IsDescending(false, true);
        modelBuilder.Entity<Defect>().HasIndex(d => new { d.ClinicId, d.Severity, d.Status }).HasFilter("status IN ('reported','investigating')");
        modelBuilder.Entity<Defect>().Property(d => d.RowVersion).IsConcurrencyToken();

        modelBuilder.Entity<SpcObservation>().HasKey(so => new { so.Id, so.ObservationDate });
        modelBuilder.Entity<SpcObservation>().Property(so => so.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<SpcObservation>().HasIndex(so => new { so.ProcessId, so.ObservationDate }).IsDescending(false, true);
        modelBuilder.Entity<SpcObservation>().HasIndex(so => new { so.ProcessId, so.IsOutlier }).HasFilter("is_outlier");

        modelBuilder.Entity<SpcRollup>().HasKey(sr => sr.Id);
        modelBuilder.Entity<SpcRollup>().Property(sr => sr.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<SpcRollup>().Property(sr => sr.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<SpcRollup>().HasIndex(sr => new { sr.ProcessId, sr.RollupDate }).IsDescending(false, true);
        modelBuilder.Entity<SpcRollup>().HasIndex(sr => new { sr.ClinicId, sr.OutOfControl }).HasFilter("out_of_control");
        modelBuilder.Entity<SpcRollup>().HasIndex(sr => new { sr.ClinicId, sr.ProcessId, sr.RollupDate, sr.RollupPeriod }).IsUnique();
        modelBuilder.Entity<SpcRollup>().Property(sr => sr.RowVersion).IsConcurrencyToken();

        modelBuilder.Entity<ControlChartPoint>().HasKey(ccp => ccp.Id);
        modelBuilder.Entity<ControlChartPoint>().Property(ccp => ccp.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<ControlChartPoint>().Property(ccp => ccp.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<ControlChartPoint>().HasIndex(ccp => new { ccp.ProcessId, ccp.PointDate }).IsDescending(false, true);
        modelBuilder.Entity<ControlChartPoint>().HasIndex(ccp => new { ccp.ProcessId, ccp.IsOutOfControl }).HasFilter("is_out_of_control");

        modelBuilder.Entity<FmeaItem>().HasKey(fi => fi.Id);
        modelBuilder.Entity<FmeaItem>().Property(fi => fi.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<FmeaItem>().Property(fi => fi.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<FmeaItem>().Property(fi => fi.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<FmeaItem>().HasIndex(fi => fi.ProcessId);
        modelBuilder.Entity<FmeaItem>().HasIndex(fi => new { fi.ClinicId, fi.RpnValue }).IsDescending(false, true);
        modelBuilder.Entity<FmeaItem>().Property(fi => fi.RowVersion).IsConcurrencyToken();

        modelBuilder.Entity<Project>().HasKey(p => p.Id);
        modelBuilder.Entity<Project>().Property(p => p.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<Project>().Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<Project>().Property(p => p.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<Project>().Property(p => p.TeamMembers).HasColumnType("uuid[]");
        modelBuilder.Entity<Project>().HasIndex(p => new { p.ClinicId, p.ProjectCode }).IsUnique();
        modelBuilder.Entity<Project>().HasIndex(p => new { p.ClinicId, p.Status });
        modelBuilder.Entity<Project>().Property(p => p.RowVersion).IsConcurrencyToken();

        modelBuilder.Entity<KaizenSuggestion>().HasKey(ks => ks.Id);
        modelBuilder.Entity<KaizenSuggestion>().Property(ks => ks.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<KaizenSuggestion>().Property(ks => ks.SuggestedDate).HasDefaultValueSql("CURRENT_DATE");
        modelBuilder.Entity<KaizenSuggestion>().Property(ks => ks.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<KaizenSuggestion>().HasIndex(ks => new { ks.ProcessId, ks.SuggestedDate }).IsDescending(false, true);
        modelBuilder.Entity<KaizenSuggestion>().HasIndex(ks => new { ks.ClinicId, ks.Status });

        modelBuilder.Entity<ProcessTrendSummary>().HasKey(pts => pts.Id);
        modelBuilder.Entity<ProcessTrendSummary>().Property(pts => pts.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<ProcessTrendSummary>().Property(pts => pts.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<ProcessTrendSummary>().HasIndex(pts => new { pts.ClinicId, pts.ProcessAlertLevel });
        modelBuilder.Entity<ProcessTrendSummary>().HasIndex(pts => new { pts.ClinicId, pts.ProcessId, pts.SummaryDate }).IsUnique();
    }

    private static string ToSnakeCase(string input) { if (string.IsNullOrEmpty(input)) return input; var sb = new System.Text.StringBuilder(); for (int i = 0; i < input.Length; i++) { var c = input[i]; if (char.IsUpper(c)) { if (i > 0 && !char.IsUpper(input[i - 1])) sb.Append('_'); sb.Append(char.ToLowerInvariant(c)); } else sb.Append(c); } return sb.ToString(); }
}
