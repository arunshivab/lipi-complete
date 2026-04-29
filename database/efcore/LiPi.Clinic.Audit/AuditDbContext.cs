using Microsoft.EntityFrameworkCore;
using LiPi.Clinic.Audit.Entities;

namespace LiPi.Clinic.Audit;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<HashChainTip> HashChainTips => Set<HashChainTip>();
    public DbSet<PhiAccessLog> PhiAccessLogs => Set<PhiAccessLog>();
    public DbSet<ExportLog> ExportLogs => Set<ExportLog>();
    public DbSet<PrintLog> PrintLogs => Set<PrintLog>();
    public DbSet<BlockchainAnchor> BlockchainAnchors => Set<BlockchainAnchor>();
    public DbSet<HashChainVerificationRun> HashChainVerificationRuns => Set<HashChainVerificationRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("audit");

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
            foreach (var property in entity.GetProperties())
                property.SetColumnName(ToSnakeCase(property.Name ?? string.Empty));

        // EventType (reference, no RLS)
        modelBuilder.Entity<EventType>().HasKey(et => et.Code);

        // AuditEvent
        modelBuilder.Entity<AuditEvent>().HasKey(ae => new { ae.Id, ae.EventTs });
        modelBuilder.Entity<AuditEvent>().Property(ae => ae.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<AuditEvent>().Property(ae => ae.EventTs).HasDefaultValueSql("now()");
        modelBuilder.Entity<AuditEvent>().Property(ae => ae.ActorRoleCodes).HasColumnType("text[]");
        modelBuilder.Entity<AuditEvent>().Property(ae => ae.ActorIp)
;  // IPAddress→inet handled natively by Npgsql 10
        modelBuilder.Entity<AuditEvent>().Property(ae => ae.BeforeState).HasColumnType("jsonb");
        modelBuilder.Entity<AuditEvent>().Property(ae => ae.AfterState).HasColumnType("jsonb");
        modelBuilder.Entity<AuditEvent>().HasIndex(ae => new { ae.ClinicId, ae.EventTs }).IsDescending(false, true);
        modelBuilder.Entity<AuditEvent>().HasIndex(ae => new { ae.EntityType, ae.EntityId });
        modelBuilder.Entity<AuditEvent>().HasIndex(ae => new { ae.ActorUserId, ae.EventTs }).IsDescending(false, true);

        // HashChainTip
        modelBuilder.Entity<HashChainTip>().HasKey(hct => hct.ClinicId);
        modelBuilder.Entity<HashChainTip>().Property(hct => hct.UpdatedAt).HasDefaultValueSql("now()");

        // PhiAccessLog
        modelBuilder.Entity<PhiAccessLog>().HasKey(pal => new { pal.Id, pal.AccessedAt });
        modelBuilder.Entity<PhiAccessLog>().Property(pal => pal.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<PhiAccessLog>().Property(pal => pal.AccessedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<PhiAccessLog>().Property(pal => pal.ActorRoleCodes).HasColumnType("text[]");
        modelBuilder.Entity<PhiAccessLog>().Property(pal => pal.ActorIp)
;  // IPAddress→inet handled natively by Npgsql 10
        modelBuilder.Entity<PhiAccessLog>().HasIndex(pal => new { pal.PatientId, pal.AccessedAt }).IsDescending(false, true);
        modelBuilder.Entity<PhiAccessLog>().HasIndex(pal => new { pal.ActorUserId, pal.AccessedAt }).IsDescending(false, true);

        // ExportLog
        modelBuilder.Entity<ExportLog>().HasKey(el => el.Id);
        modelBuilder.Entity<ExportLog>().Property(el => el.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<ExportLog>().Property(el => el.ExportedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<ExportLog>().Property(el => el.PatientIds).HasColumnType("uuid[]");
        modelBuilder.Entity<ExportLog>().Property(el => el.QueryParameters).HasColumnType("jsonb");
        modelBuilder.Entity<ExportLog>().HasIndex(el => new { el.ActorUserId, el.ExportedAt }).IsDescending(false, true);

        // PrintLog
        modelBuilder.Entity<PrintLog>().HasKey(pl => pl.Id);
        modelBuilder.Entity<PrintLog>().Property(pl => pl.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<PrintLog>().Property(pl => pl.PrintedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<PrintLog>().Property(pl => pl.Copies).HasDefaultValue(1);
        modelBuilder.Entity<PrintLog>().Property(pl => pl.WatermarkApplied).HasDefaultValue(true);
        modelBuilder.Entity<PrintLog>().Property(pl => pl.ClientIp)
;  // IPAddress→inet handled natively by Npgsql 10
        modelBuilder.Entity<PrintLog>().HasIndex(pl => new { pl.PatientId, pl.PrintedAt }).IsDescending(false, true);

        // BlockchainAnchor
        modelBuilder.Entity<BlockchainAnchor>().HasKey(ba => ba.Id);
        modelBuilder.Entity<BlockchainAnchor>().Property(ba => ba.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<BlockchainAnchor>().Property(ba => ba.AnchoredAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<BlockchainAnchor>().HasIndex(ba => new { ba.ClinicId, ba.AnchorType, ba.WindowTo }).IsDescending(false, false, true);

        // HashChainVerificationRun
        modelBuilder.Entity<HashChainVerificationRun>().HasKey(hcvr => hcvr.Id);
        modelBuilder.Entity<HashChainVerificationRun>().Property(hcvr => hcvr.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<HashChainVerificationRun>().Property(hcvr => hcvr.StartedAt).HasDefaultValueSql("now()");
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && !char.IsUpper(input[i - 1]))
                    sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
                sb.Append(c);
        }
        return sb.ToString();
    }
}
