using LiPi.Clinic.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiPi.Clinic.Core;

/// <summary>
/// EF Core DbContext for a clinic's OLTP DB — <c>core</c> schema only (Wave 1).
/// Additional schemas (identity, abdm, audit, ...) are layered in as sibling DbContexts
/// and share this same physical database via multi-DbContext sharing.
/// Connection string is resolved per-request by the TenantResolver middleware from
/// the JWT's <c>clinic_id</c> claim + master.clinic_databases lookup.
/// The SET LOCAL lipi.clinic_id = '...' is issued by the connection interceptor to
/// activate row-level-security policies defined in 01_core.sql.
/// </summary>
public class ClinicCoreDbContext : DbContext
{
    public ClinicCoreDbContext(DbContextOptions<ClinicCoreDbContext> options) : base(options) { }

    // Reference / codesets
    public DbSet<Country>           Countries           => Set<Country>();
    public DbSet<State>             States              => Set<State>();
    public DbSet<City>              Cities              => Set<City>();
    public DbSet<Pincode>           Pincodes            => Set<Pincode>();
    public DbSet<Address>           Addresses           => Set<Address>();
    public DbSet<Icd10Code>         Icd10Codes          => Set<Icd10Code>();
    public DbSet<SnomedCode>        SnomedCodes         => Set<SnomedCode>();
    public DbSet<LoincCode>         LoincCodes          => Set<LoincCode>();
    public DbSet<RxNormCode>        RxNormCodes         => Set<RxNormCode>();
    public DbSet<CdscoDeviceClass>  CdscoDeviceClasses  => Set<CdscoDeviceClass>();

    // Facility topology
    public DbSet<Facility>          Facilities          => Set<Facility>();
    public DbSet<Department>        Departments         => Set<Department>();
    public DbSet<Specialty>         Specialties         => Set<Specialty>();
    public DbSet<Ward>              Wards               => Set<Ward>();
    public DbSet<Room>              Rooms               => Set<Room>();
    public DbSet<Bed>                Beds                => Set<Bed>();

    // Identity core
    public DbSet<Person>            Persons             => Set<Person>();
    public DbSet<ContactPoint>      ContactPoints       => Set<ContactPoint>();
    public DbSet<PersonAddress>     PersonAddresses     => Set<PersonAddress>();
    public DbSet<Patient>           Patients            => Set<Patient>();
    public DbSet<PatientIdentifier> PatientIdentifiers  => Set<PatientIdentifier>();
    public DbSet<EmergencyContact>  EmergencyContacts   => Set<EmergencyContact>();
    public DbSet<Staff>             Staff               => Set<Staff>();
    public DbSet<StaffDepartment>   StaffDepartments    => Set<StaffDepartment>();
    public DbSet<Consent>           Consents            => Set<Consent>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasDefaultSchema("core");

        // snake_case convention — matches hand-authored DDL
        foreach (var entity in b.Model.GetEntityTypes())
        {
            foreach (var prop in entity.GetProperties())
                prop.SetColumnName(ToSnakeCase(prop.Name));
        }

        // ---- Primary keys ----
        b.Entity<Country>().HasKey(x => x.Code);
        b.Entity<Icd10Code>().HasKey(x => x.Code);
        b.Entity<SnomedCode>().HasKey(x => x.Code);
        b.Entity<LoincCode>().HasKey(x => x.Code);
        b.Entity<RxNormCode>().HasKey(x => x.Rxcui);
        b.Entity<CdscoDeviceClass>().HasKey(x => x.Code);
        b.Entity<StaffDepartment>().HasKey(x => new { x.StaffId, x.DepartmentId });

        // ---- Pincode.Value column name override ('pincode' in DB) ----
        b.Entity<Pincode>().Property(x => x.Value).HasColumnName("pincode");

        // ---- Person.DisplayName is a STORED GENERATED column — read-only ----
        b.Entity<Person>()
            .Property(x => x.DisplayName)
            .ValueGeneratedOnAddOrUpdate()
            .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

        // ---- Patient : Person 1:1 ----
        b.Entity<Patient>()
            .HasOne(x => x.Person).WithOne(p => p.Patient!)
            .HasForeignKey<Patient>(x => x.PersonId);

        b.Entity<Staff>()
            .HasOne(x => x.Person).WithOne(p => p.Staff!)
            .HasForeignKey<Staff>(x => x.PersonId);

        // ---- EmergencyContact column name: AddressJson → 'address' ----
        b.Entity<EmergencyContact>().Property(x => x.AddressJson).HasColumnName("address");

        // ---- JSONB columns ----
        b.Entity<Facility>().Property(x => x.Contact).HasColumnType("jsonb");
        b.Entity<Person>().Property(x => x.ExtensionData).HasColumnType("jsonb");
        b.Entity<Patient>().Property(x => x.ActiveAlerts).HasColumnType("jsonb");
        b.Entity<Patient>().Property(x => x.ExtensionData).HasColumnType("jsonb");
        b.Entity<Staff>().Property(x => x.ExtensionData).HasColumnType("jsonb");
        b.Entity<Consent>().Property(x => x.Scope).HasColumnType("jsonb");
        b.Entity<EmergencyContact>().Property(x => x.AddressJson).HasColumnType("jsonb");

        // ---- Concurrency ----
        foreach (var et in b.Model.GetEntityTypes())
        {
            var rv = et.FindProperty("RowVersion");
            if (rv != null) rv.IsConcurrencyToken = true;
        }

        // ---- Soft-delete filter ----
        b.Entity<Person>().HasQueryFilter(x => x.DeletedAt == null);
        b.Entity<Patient>().HasQueryFilter(x => x.DeletedAt == null);
        b.Entity<Staff>().HasQueryFilter(x => x.DeletedAt == null);
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
