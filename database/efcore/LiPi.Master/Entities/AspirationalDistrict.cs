using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Master.Entities;

/// <summary>
/// NITI Aayog Aspirational Districts Programme (ADP).
/// Source: Government of India notification, launched 2018-01-05.
/// No hard deletes — use IsActive = false to retire a district.
/// AnnouncementDate = date the district was officially notified under ADP.
/// Used in patient reports: visit_date vs announcement_date determines
/// whether a patient visit counts as "during aspirational period".
/// </summary>
[Table("aspirational_districts", Schema = "master")]
public class AspirationalDistrict
{
    public Guid   Id               { get; set; }
    public string DistrictName     { get; set; } = default!;   // Title-case, e.g. "Washim"
    public string StateName        { get; set; } = default!;   // Title-case, e.g. "Maharashtra"
    public DateOnly AnnouncementDate { get; set; }             // Official ADP notification date
    public bool   IsActive         { get; set; } = true;       // false = retired (renamed/graduated)
    public string? Notes           { get; set; }               // e.g. "Renamed from Osmanabad 2023"
    public string  DataSource      { get; set; } = "seed";     // seed | manual | gazette
    public DateTimeOffset CreatedAt  { get; set; }
    public DateTimeOffset UpdatedAt  { get; set; }
    public Guid?  UpdatedByUserId  { get; set; }               // who last toggled / added
}
