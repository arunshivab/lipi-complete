using Microsoft.EntityFrameworkCore;
using LiPi.Clinic.Abdm.Entities;

namespace LiPi.Clinic.Abdm;

public class AbdmDbContext : DbContext
{
    public AbdmDbContext(DbContextOptions<AbdmDbContext> options) : base(options) { }

    public DbSet<FacilityRegistry> FacilityRegistries => Set<FacilityRegistry>();
    public DbSet<ProfessionalRegistry> ProfessionalRegistries => Set<ProfessionalRegistry>();
    public DbSet<AbhaProfile> AbhaProfiles => Set<AbhaProfile>();
    public DbSet<LinkRequest> LinkRequests => Set<LinkRequest>();
    public DbSet<CareContext> CareContexts => Set<CareContext>();
    public DbSet<ConsentRequest> ConsentRequests => Set<ConsentRequest>();
    public DbSet<ConsentArtefact> ConsentArtefacts => Set<ConsentArtefact>();
    public DbSet<HiExchangeSession> HiExchangeSessions => Set<HiExchangeSession>();
    public DbSet<HiCareBundleLog> HiCareBundles => Set<HiCareBundleLog>();
    public DbSet<GatewayRequest> GatewayRequests => Set<GatewayRequest>();
    public DbSet<AadhaarEkycRequest> AadhaarEkycRequests => Set<AadhaarEkycRequest>();
    public DbSet<SubscriptionCallback> SubscriptionCallbacks => Set<SubscriptionCallback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("abdm");

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
                property.SetColumnName(ToSnakeCase(property.Name));
            foreach (var key in entity.GetKeys())
                key.SetName(ToSnakeCase(key.GetName() ?? string.Empty));
            foreach (var fk in entity.GetForeignKeys())
                fk.SetConstraintName(ToSnakeCase(fk.GetConstraintName() ?? string.Empty));
        }

        // FacilityRegistry
        modelBuilder.Entity<FacilityRegistry>().HasKey(fr => fr.Id);
        modelBuilder.Entity<FacilityRegistry>().Property(fr => fr.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<FacilityRegistry>().Property(fr => fr.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<FacilityRegistry>().Property(fr => fr.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<FacilityRegistry>().Property(fr => fr.RegistryPayload).HasColumnType("jsonb");
        modelBuilder.Entity<FacilityRegistry>().HasIndex(fr => new { fr.ClinicId, fr.HfrFacilityId }).IsUnique();
        modelBuilder.Entity<FacilityRegistry>().Property(fr => fr.RowVersion).IsConcurrencyToken();

        // ProfessionalRegistry
        modelBuilder.Entity<ProfessionalRegistry>().HasKey(pr => pr.Id);
        modelBuilder.Entity<ProfessionalRegistry>().Property(pr => pr.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<ProfessionalRegistry>().Property(pr => pr.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<ProfessionalRegistry>().Property(pr => pr.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<ProfessionalRegistry>().Property(pr => pr.RegistryPayload).HasColumnType("jsonb");
        modelBuilder.Entity<ProfessionalRegistry>().HasIndex(pr => new { pr.ClinicId, pr.HprId }).IsUnique();
        modelBuilder.Entity<ProfessionalRegistry>().Property(pr => pr.RowVersion).IsConcurrencyToken();

        // AbhaProfile
        modelBuilder.Entity<AbhaProfile>().HasKey(ap => ap.Id);
        modelBuilder.Entity<AbhaProfile>().Property(ap => ap.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<AbhaProfile>().Property(ap => ap.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<AbhaProfile>().Property(ap => ap.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<AbhaProfile>().Property(ap => ap.LinkedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<AbhaProfile>().Property(ap => ap.ProfilePayload).HasColumnType("jsonb");
        modelBuilder.Entity<AbhaProfile>().HasIndex(ap => ap.AbhaNumber);
        modelBuilder.Entity<AbhaProfile>().Property(ap => ap.RowVersion).IsConcurrencyToken();

        // LinkRequest
        modelBuilder.Entity<LinkRequest>().HasKey(lr => lr.Id);
        modelBuilder.Entity<LinkRequest>().Property(lr => lr.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<LinkRequest>().Property(lr => lr.RequestedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<LinkRequest>().HasOne(lr => lr.AbhaProfile).WithMany(ap => ap.LinkRequests)
            .HasForeignKey(lr => lr.AbhaProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<LinkRequest>().HasIndex(lr => lr.PatientId);

        // CareContext
        modelBuilder.Entity<CareContext>().HasKey(cc => cc.Id);
        modelBuilder.Entity<CareContext>().Property(cc => cc.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<CareContext>().Property(cc => cc.LinkedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<CareContext>().Property(cc => cc.Visible).HasDefaultValue(true);
        modelBuilder.Entity<CareContext>().HasOne(cc => cc.AbhaProfile).WithMany(ap => ap.CareContexts)
            .HasForeignKey(cc => cc.AbhaProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CareContext>().HasIndex(cc => new { cc.AbhaProfileId, cc.UnlinkedAt }).HasFilter("unlinked_at IS NULL");

        // ConsentRequest
        modelBuilder.Entity<ConsentRequest>().HasKey(cr => cr.Id);
        modelBuilder.Entity<ConsentRequest>().Property(cr => cr.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<ConsentRequest>().Property(cr => cr.RaisedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<ConsentRequest>().Property(cr => cr.HiTypes).HasColumnType("text[]");
        modelBuilder.Entity<ConsentRequest>().Property(cr => cr.ConsentPayload).HasColumnType("jsonb");
        modelBuilder.Entity<ConsentRequest>().HasOne(cr => cr.AbhaProfile).WithMany(ap => ap.ConsentRequests)
            .HasForeignKey(cr => cr.AbhaProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ConsentRequest>().HasIndex(cr => new { cr.AbhaProfileId });
        modelBuilder.Entity<ConsentRequest>().HasIndex(cr => new { cr.ClinicId, cr.Status });

        // ConsentArtefact
        modelBuilder.Entity<ConsentArtefact>().HasKey(ca => ca.Id);
        modelBuilder.Entity<ConsentArtefact>().Property(ca => ca.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<ConsentArtefact>().Property(ca => ca.IssuedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<ConsentArtefact>().Property(ca => ca.ArtefactPayload).HasColumnType("jsonb");
        modelBuilder.Entity<ConsentArtefact>().HasOne(ca => ca.ConsentRequest).WithMany(cr => cr.ConsentArtefacts)
            .HasForeignKey(ca => ca.ConsentRequestId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ConsentArtefact>().HasIndex(ca => ca.ArtefactId).IsUnique();

        // HiExchangeSession
        modelBuilder.Entity<HiExchangeSession>().HasKey(hes => hes.Id);
        modelBuilder.Entity<HiExchangeSession>().Property(hes => hes.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<HiExchangeSession>().Property(hes => hes.RequestedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<HiExchangeSession>().Property(hes => hes.KeyMaterial).HasColumnType("jsonb");
        modelBuilder.Entity<HiExchangeSession>().HasOne(hes => hes.ConsentArtefact).WithMany(ca => ca.HiExchangeSessions)
            .HasForeignKey(hes => hes.ConsentArtefactId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<HiExchangeSession>().HasIndex(hes => hes.TransactionId).IsUnique();

        // HiCareBundleLog
        modelBuilder.Entity<HiCareBundleLog>().HasKey(hcbl => hcbl.Id);
        modelBuilder.Entity<HiCareBundleLog>().Property(hcbl => hcbl.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<HiCareBundleLog>().Property(hcbl => hcbl.SentAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<HiCareBundleLog>().HasOne(hcbl => hcbl.HiExchangeSession).WithMany(hes => hes.HiCareBundles)
            .HasForeignKey(hcbl => hcbl.HiExchangeSessionId).OnDelete(DeleteBehavior.Cascade);

        // GatewayRequest
        modelBuilder.Entity<GatewayRequest>().HasKey(gr => gr.Id);
        modelBuilder.Entity<GatewayRequest>().Property(gr => gr.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<GatewayRequest>().Property(gr => gr.RequestedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<GatewayRequest>().Property(gr => gr.RequestBody).HasColumnType("jsonb");
        modelBuilder.Entity<GatewayRequest>().Property(gr => gr.ResponseBody).HasColumnType("jsonb");
        modelBuilder.Entity<GatewayRequest>().HasIndex(gr => gr.CorrelationId);
        modelBuilder.Entity<GatewayRequest>().HasIndex(gr => new { gr.ClinicId, gr.Outcome, gr.RequestedAt }).IsDescending(false, false, true);

        // AadhaarEkycRequest
        modelBuilder.Entity<AadhaarEkycRequest>().HasKey(aer => aer.Id);
        modelBuilder.Entity<AadhaarEkycRequest>().Property(aer => aer.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<AadhaarEkycRequest>().Property(aer => aer.RequestedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<AadhaarEkycRequest>().HasIndex(aer => aer.TxnId).IsUnique();
        modelBuilder.Entity<AadhaarEkycRequest>().HasIndex(aer => aer.PatientId);
        modelBuilder.Entity<AadhaarEkycRequest>().HasIndex(aer => aer.StaffId);

        // SubscriptionCallback
        modelBuilder.Entity<SubscriptionCallback>().HasKey(sc => sc.Id);
        modelBuilder.Entity<SubscriptionCallback>().Property(sc => sc.Id).HasDefaultValueSql("core.uuid_v7()");
        modelBuilder.Entity<SubscriptionCallback>().Property(sc => sc.CreatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<SubscriptionCallback>().Property(sc => sc.UpdatedAt).HasDefaultValueSql("now()");
        modelBuilder.Entity<SubscriptionCallback>().Property(sc => sc.HiTypes).HasColumnType("text[]");
        modelBuilder.Entity<SubscriptionCallback>().HasOne(sc => sc.AbhaProfile).WithMany()
            .HasForeignKey(sc => sc.AbhaProfileId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SubscriptionCallback>().HasIndex(sc => sc.SubscriptionId).IsUnique();
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
