using LiPi.Clinic.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiPi.Clinic.Core;

public class ClinicCoreDbContext : DbContext
{
    public ClinicCoreDbContext(DbContextOptions<ClinicCoreDbContext> opts) : base(opts) { }

    // ── DbSets ──────────────────────────────────────────────────────────
    public DbSet<Patient>          Patients          => Set<Patient>();
    public DbSet<ContactPoint>     ContactPoints     => Set<ContactPoint>();
    public DbSet<Address>          Addresses         => Set<Address>();
    public DbSet<PatientIdentifier>PatientIdentifiers=> Set<PatientIdentifier>();
    public DbSet<PatientPayer>     PatientPayers     => Set<PatientPayer>();
    public DbSet<EmergencyContact> EmergencyContacts => Set<EmergencyContact>();
    public DbSet<FlagDefinition>   FlagDefinitions   => Set<FlagDefinition>();
    public DbSet<PatientFlag>      PatientFlags      => Set<PatientFlag>();
    public DbSet<Consent>          Consents          => Set<Consent>();
    public DbSet<GeoState>         GeoStates         => Set<GeoState>();
    public DbSet<GeoDistrict>      GeoDistricts      => Set<GeoDistrict>();
    public DbSet<GeoCity>          GeoCities         => Set<GeoCity>();
    public DbSet<Country>          Countries         => Set<Country>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // ── Patient ──────────────────────────────────────────────────────
        b.Entity<Patient>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("core.uuid_v7()");
            e.Property(x => x.ValidFrom).HasDefaultValueSql("now()");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
            e.Property(x => x.ExtensionData).HasColumnType("jsonb");

            // DisplayName is a GENERATED STORED column — never write it
            e.Property(x => x.DisplayName).ValueGeneratedOnAddOrUpdate();

            // Unique index on EntityId where current (valid_to IS NULL) enforced at DB level
            // EF cannot express partial unique indexes — enforced by SQL schema

            // Navigations from Patient to dependents via EntityId (app-level, no DB FK)
            // EF navigations defined here for query convenience
            e.HasMany(x => x.ContactPoints)
             .WithOne()
             .HasForeignKey(c => c.PatientEntityId)
             .HasPrincipalKey(p => p.EntityId)
             .IsRequired(false);

            e.HasMany(x => x.Addresses)
             .WithOne()
             .HasForeignKey(a => a.PatientEntityId)
             .HasPrincipalKey(p => p.EntityId)
             .IsRequired(false);

            e.HasMany(x => x.Identifiers)
             .WithOne()
             .HasForeignKey(i => i.PatientEntityId)
             .HasPrincipalKey(p => p.EntityId)
             .IsRequired(false);

            e.HasMany(x => x.Payers)
             .WithOne()
             .HasForeignKey(py => py.PatientEntityId)
             .HasPrincipalKey(p => p.EntityId)
             .IsRequired(false);

            e.HasMany(x => x.EmergencyContacts)
             .WithOne()
             .HasForeignKey(ec => ec.PatientEntityId)
             .HasPrincipalKey(p => p.EntityId)
             .IsRequired(false);

            e.HasMany(x => x.Flags)
             .WithOne()
             .HasForeignKey(f => f.PatientEntityId)
             .HasPrincipalKey(p => p.EntityId)
             .IsRequired(false);

            e.HasMany(x => x.Consents)
             .WithOne()
             .HasForeignKey(c => c.PatientEntityId)
             .HasPrincipalKey(p => p.EntityId)
             .IsRequired(false);

            // Only show current versions by default
            e.HasQueryFilter(x => x.ValidTo == null);
        });

        // ── ContactPoint ─────────────────────────────────────────────────
        b.Entity<ContactPoint>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("core.uuid_v7()");
            e.Property(x => x.ValidFrom).HasDefaultValueSql("now()");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            e.HasQueryFilter(x => x.ValidTo == null);
        });

        // ── Address ──────────────────────────────────────────────────────
        b.Entity<Address>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("core.uuid_v7()");
            e.Property(x => x.ValidFrom).HasDefaultValueSql("now()");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            e.HasQueryFilter(x => x.ValidTo == null);
        });

        // ── PatientIdentifier ────────────────────────────────────────────
        b.Entity<PatientIdentifier>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("core.uuid_v7()");
            e.Property(x => x.ValidFrom).HasDefaultValueSql("now()");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            e.HasQueryFilter(x => x.ValidTo == null);
        });

        // ── PatientPayer ─────────────────────────────────────────────────
        b.Entity<PatientPayer>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("core.uuid_v7()");
            e.Property(x => x.ValidFrom).HasDefaultValueSql("now()");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            e.HasQueryFilter(x => x.ValidTo == null);
        });

        // ── EmergencyContact ─────────────────────────────────────────────
        b.Entity<EmergencyContact>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("core.uuid_v7()");
            e.Property(x => x.ValidFrom).HasDefaultValueSql("now()");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            e.HasQueryFilter(x => x.ValidTo == null);
        });

        // ── FlagDefinition ───────────────────────────────────────────────
        b.Entity<FlagDefinition>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("core.uuid_v7()");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
            e.HasQueryFilter(x => x.IsActive);
        });

        // ── PatientFlag ──────────────────────────────────────────────────
        b.Entity<PatientFlag>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("core.uuid_v7()");
            e.Property(x => x.FlaggedAt).HasDefaultValueSql("now()");
            e.HasOne(x => x.Flag).WithMany()
             .HasForeignKey(x => x.FlagId).IsRequired();
        });

        // ── Consent ──────────────────────────────────────────────────────
        b.Entity<Consent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("core.uuid_v7()");
            e.Property(x => x.Scope).HasColumnType("jsonb");
            e.Property(x => x.GrantedAt).HasDefaultValueSql("now()");
            e.Property(x => x.ValidFrom).HasDefaultValueSql("now()");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        });

        // ── Geo reference tables ─────────────────────────────────────────
        b.Entity<Country>().HasKey(x => x.Code);
        b.Entity<GeoState>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("core.uuid_v7()");
            e.HasMany(x => x.Districts).WithOne(d => d.State)
             .HasForeignKey(d => d.StateId);
        });
        b.Entity<GeoDistrict>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("core.uuid_v7()");
            e.HasMany(x => x.Cities).WithOne(c => c.District)
             .HasForeignKey(c => c.DistrictId);
        });
        b.Entity<GeoCity>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("core.uuid_v7()");
        });
        // ── Snake_case column naming (no external package needed) ───────────
        // Converts every C# PascalCase property to snake_case column name
        // e.g. EntityId → entity_id, ValidFrom → valid_from, FirstName → first_name
        foreach (var entity in b.Model.GetEntityTypes())
        {
            foreach (var prop in entity.GetProperties())
            {
                prop.SetColumnName(ToSnakeCase(prop.Name));
            }
            foreach (var key in entity.GetKeys())
                key.SetName(ToSnakeCase(key.GetName() ?? string.Empty));
            foreach (var fk in entity.GetForeignKeys())
                fk.SetConstraintName(ToSnakeCase(fk.GetConstraintName() ?? string.Empty));
            foreach (var idx in entity.GetIndexes())
                idx.SetDatabaseName(ToSnakeCase(idx.GetDatabaseName() ?? string.Empty));
        }
    }

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && i > 0)
                sb.Append('_');
            sb.Append(char.ToLower(name[i]));
        }
        return sb.ToString();
    }
}
