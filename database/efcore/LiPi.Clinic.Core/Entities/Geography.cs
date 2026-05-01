using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Clinic.Core.Entities;

// Reference geodata — seeded from 02_geodata_seed.sql
// These are read-only lookup tables — no patient data here.

[Table("countries", Schema = "core")]
public class Country
{
    public string Code     { get; set; } = default!;   // ISO 3166-1 alpha-2
    public string Name     { get; set; } = default!;
    public string Iso3     { get; set; } = default!;
    public string DialCode { get; set; } = default!;
    public bool   IsActive { get; set; } = true;
}

[Table("states", Schema = "core")]
public class GeoState
{
    public Guid   Id       { get; set; }
    public string Name     { get; set; } = default!;
    public string Code     { get; set; } = default!;   // ISO 3166-2:IN e.g. IN-MH
    public bool   IsUt     { get; set; }               // true = Union Territory
    public bool   IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<GeoDistrict> Districts { get; set; } = new List<GeoDistrict>();
}

[Table("districts", Schema = "core")]
public class GeoDistrict
{
    public Guid   Id             { get; set; }
    public Guid   StateId        { get; set; }
    public string Name           { get; set; } = default!;
    public bool   IsAspirational { get; set; }
    public bool   IsActive       { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public GeoState State        { get; set; } = default!;
    public ICollection<GeoCity> Cities { get; set; } = new List<GeoCity>();
}

[Table("cities", Schema = "core")]
public class GeoCity
{
    public Guid   Id           { get; set; }
    public Guid   DistrictId   { get; set; }
    public Guid   StateId      { get; set; }
    public string Name         { get; set; } = default!;
    public bool   IsDistrictHq { get; set; }
    public bool   IsActive     { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }

    public GeoDistrict District { get; set; } = default!;
}

// NOTE: Address class lives in Person.cs (new v3 immutable address table)
// NOTE: Old Pincode entity removed — pincodes stored as plain text in Address.Pincode
