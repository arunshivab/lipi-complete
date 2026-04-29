using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Clinic.Core.Entities;

[Table("countries", Schema = "core")]
public class Country
{
    public string Code { get; set; } = default!;             // ISO-3166 alpha-2
    public string Name { get; set; } = default!;
    public string Iso3 { get; set; } = default!;
    public string DialCode { get; set; } = default!;
}

[Table("states", Schema = "core")]
public class State
{
    public Guid Id { get; set; }
    public string CountryCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;

    public Country Country { get; set; } = default!;
    public ICollection<City> Cities { get; set; } = new List<City>();
}

[Table("cities", Schema = "core")]
public class City
{
    public Guid Id { get; set; }
    public Guid StateId { get; set; }
    public string Name { get; set; } = default!;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public State State { get; set; } = default!;
    public ICollection<Pincode> Pincodes { get; set; } = new List<Pincode>();
}

[Table("pincodes", Schema = "core")]
public class Pincode
{
    public Guid Id { get; set; }
    public Guid CityId { get; set; }
    public string Value { get; set; } = default!;            // mapped to 'pincode' column
    public string? Area { get; set; }

    public City City { get; set; } = default!;
}

[Table("addresses", Schema = "core")]
public class Address
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string UseType { get; set; } = default!;          // home | work | temp | old | billing | emergency
    public string Line1 { get; set; } = default!;
    public string? Line2 { get; set; }
    public string? Landmark { get; set; }
    public Guid? CityId { get; set; }
    public Guid? StateId { get; set; }
    public string? Pincode { get; set; }
    public string CountryCode { get; set; } = "IN";
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
