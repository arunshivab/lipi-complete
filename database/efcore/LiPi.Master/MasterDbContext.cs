 using LiPi.Master.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiPi.Master;

/// <summary>
/// EF Core DbContext for the LiPi Master Registry database.
/// One instance per region — never scoped per tenant.
/// Corresponds to DDL in <c>database/master/001_schema_master.sql</c>.
/// </summary>
public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }

    public DbSet<Organization>                Organizations        => Set<Organization>();
    public DbSet<Clinic>                      Clinics              => Set<Clinic>();
    public DbSet<ClinicDatabase>              ClinicDatabases      => Set<ClinicDatabase>();
    public DbSet<ClinicGroup>                 ClinicGroups         => Set<ClinicGroup>();
    public DbSet<ClinicGroupMember>           ClinicGroupMembers   => Set<ClinicGroupMember>();
    public DbSet<SubscriptionPlan>            SubscriptionPlans    => Set<SubscriptionPlan>();
    public DbSet<Subscription>                Subscriptions        => Set<Subscription>();
    public DbSet<Invoice>                     Invoices             => Set<Invoice>();
    public DbSet<GlobalUser>                  GlobalUsers          => Set<GlobalUser>();
    public DbSet<GlobalUserClinicAccess>      GlobalUserClinicAccess => Set<GlobalUserClinicAccess>();
    public DbSet<IdentityProvider>            IdentityProviders    => Set<IdentityProvider>();
    public DbSet<IdpGroupRoleMapping>         IdpGroupRoleMappings => Set<IdpGroupRoleMapping>();
    public DbSet<FeatureFlag>                 FeatureFlags         => Set<FeatureFlag>();
    public DbSet<FeatureFlagOverride>         FeatureFlagOverrides => Set<FeatureFlagOverride>();
    public DbSet<MasterAuditEvent>            AuditEvents          => Set<MasterAuditEvent>();
    public DbSet<PlatformUser>                PlatformUsers        => Set<PlatformUser>();
    public DbSet<ClinicMembership>            ClinicMemberships    => Set<ClinicMembership>();
    public DbSet<AspirationalDistrict>        AspirationalDistricts => Set<AspirationalDistrict>();
	public DbSet<BrandTheme> BrandThemes => Set<BrandTheme>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("master");

        // ---- Snake-case table + column naming ----
        foreach (var entity in b.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
                entity.SetTableName(ToSnakeCase(tableName));

            foreach (var prop in entity.GetProperties())
                prop.SetColumnName(ToSnakeCase(prop.Name));
            foreach (var key in entity.GetKeys())
                key.SetName(ToSnakeCase(key.GetName() ?? string.Empty));
            foreach (var fk in entity.GetForeignKeys())
                fk.SetConstraintName(ToSnakeCase(fk.GetConstraintName() ?? string.Empty));
            foreach (var idx in entity.GetIndexes())
                idx.SetDatabaseName(ToSnakeCase(idx.GetDatabaseName() ?? string.Empty));
        }

        // ---- Composite keys ----
        b.Entity<ClinicGroupMember>().HasKey(x => new { x.ClinicGroupId, x.ClinicId });
        b.Entity<GlobalUserClinicAccess>().HasKey(x => new { x.GlobalUserId, x.ClinicId });

        // ---- Concurrency token via row_version ----
        foreach (var et in b.Model.GetEntityTypes())
        {
            var rv = et.FindProperty("RowVersion");
            if (rv != null) rv.IsConcurrencyToken = true;
        }

        // ---- JSONB columns ----
        b.Entity<Organization>().Property(x => x.RegisteredAddress).HasColumnType("jsonb");
        b.Entity<Organization>().Property(x => x.PrimaryContact).HasColumnType("jsonb");
        b.Entity<Organization>().Property(x => x.ExtensionData).HasColumnType("jsonb");
        b.Entity<Clinic>().Property(x => x.Address).HasColumnType("jsonb");
        b.Entity<Clinic>().Property(x => x.ExtensionData).HasColumnType("jsonb");
        b.Entity<ClinicDatabase>().Property(x => x.BackupPolicy).HasColumnType("jsonb");
        b.Entity<SubscriptionPlan>().Property(x => x.Features).HasColumnType("jsonb");
        b.Entity<Invoice>().Property(x => x.LineItems).HasColumnType("jsonb");
        b.Entity<IdentityProvider>().Property(x => x.Config).HasColumnType("jsonb");
        b.Entity<FeatureFlag>().Property(x => x.RolloutRules).HasColumnType("jsonb");
        b.Entity<MasterAuditEvent>().Property(x => x.BeforeState).HasColumnType("jsonb");
        b.Entity<MasterAuditEvent>().Property(x => x.AfterState).HasColumnType("jsonb");

        // ---- PlatformUser ----
        b.Entity<PlatformUser>().HasKey(x => x.Id);
        b.Entity<PlatformUser>().Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Entity<PlatformUser>().Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Entity<PlatformUser>().Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
        b.Entity<PlatformUser>().Property(x => x.ExtensionData).HasColumnType("jsonb");
        b.Entity<PlatformUser>().Property(x => x.Status).HasDefaultValue("active");
        b.Entity<PlatformUser>().Property(x => x.MustChangePassword).HasDefaultValue(true);
        b.Entity<PlatformUser>().Property(x => x.IsMfaEnforced).HasDefaultValue(false);
        b.Entity<PlatformUser>().Property(x => x.FailedLoginCount).HasDefaultValue(0);
        b.Entity<PlatformUser>().Property(x => x.RowVersion).IsConcurrencyToken();
        b.Entity<PlatformUser>().HasIndex(x => x.Username).IsUnique().HasFilter("deleted_at IS NULL");
        b.Entity<PlatformUser>().HasIndex(x => x.Email).HasFilter("deleted_at IS NULL");
        b.Entity<PlatformUser>().HasQueryFilter(x => x.DeletedAt == null);

        // ---- ClinicMembership ----
        b.Entity<ClinicMembership>().HasKey(x => x.Id);
        b.Entity<ClinicMembership>().Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Entity<ClinicMembership>().Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Entity<ClinicMembership>().Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
        b.Entity<ClinicMembership>().Property(x => x.Status).HasDefaultValue("active");
        b.Entity<ClinicMembership>()
            .HasOne(x => x.PlatformUser).WithMany(u => u.ClinicMemberships)
            .HasForeignKey(x => x.PlatformUserId).OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);
        b.Entity<ClinicMembership>()
            .HasOne(x => x.Clinic).WithMany()
            .HasForeignKey(x => x.ClinicId).OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);
        b.Entity<ClinicMembership>()
            .HasIndex(x => new { x.PlatformUserId, x.ClinicId }).IsUnique();

        // ---- Soft-delete query filter ----
        b.Entity<Organization>().HasQueryFilter(x => x.DeletedAt == null);
        b.Entity<Clinic>().HasQueryFilter(x => x.DeletedAt == null);
        b.Entity<ClinicGroup>().HasQueryFilter(x => x.DeletedAt == null);
        b.Entity<GlobalUser>().HasQueryFilter(x => x.DeletedAt == null);

        // ---- AspirationalDistrict ----
        b.Entity<AspirationalDistrict>().HasKey(x => x.Id);
        b.Entity<AspirationalDistrict>().Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Entity<AspirationalDistrict>().Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        b.Entity<AspirationalDistrict>().Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
        b.Entity<AspirationalDistrict>().Property(x => x.IsActive).HasDefaultValue(true);
        b.Entity<AspirationalDistrict>().Property(x => x.DataSource).HasDefaultValue("seed");
        b.Entity<AspirationalDistrict>()
            .HasIndex(x => new { x.DistrictName, x.StateName })
            .IsUnique()
            .HasDatabaseName("uq_aspirational_district_state");
        // No hard-delete query filter — all rows returned (including inactive)
		
	
b.Entity<BrandTheme>(e =>
{
    e.HasKey(x => x.BrandId);
    e.Property(x => x.BrandId).HasMaxLength(50);
    e.Property(x => x.DisplayName).HasMaxLength(100);
    e.Property(x => x.CssFilePath).HasMaxLength(200);
    e.Property(x => x.LogoLightUrl).HasMaxLength(200);
    e.Property(x => x.LogoDarkUrl).HasMaxLength(200);
});
        // NOTE: BrandTheme.BrandId → "brand_theme_id" FK on Clinic is NOT configured here
        // as a navigation property. The FK constraint lives in SQL only (migration script).
        // EF will track the Clinic.BrandThemeId scalar property for reads/writes.

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
                if (i > 0 && !char.IsUpper(input[i - 1])) sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else sb.Append(c);
        }
        return sb.ToString();
    }
}
