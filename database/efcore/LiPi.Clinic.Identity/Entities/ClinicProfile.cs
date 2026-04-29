using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Clinic.Identity.Entities;

/// <summary>
/// Per-clinic profile for a platform user.
/// Describes what the user DOES at this specific clinic.
/// References master.platform_users.id (no cross-DB FK — maintained in application layer).
/// </summary>
[Table("clinic_profiles", Schema = "identity")]
public class ClinicProfile
{
    public Guid   Id             { get; set; }
    public Guid   PlatformUserId { get; set; }  // master.platform_users.id
    public Guid   ClinicId       { get; set; }

    // ── Clinic-specific role ──────────────────────────────────────────────
    public string? Designation  { get; set; }  // HOD Radiation Oncology
    public string? Department   { get; set; }  // Radiation Oncology
    public string? StaffType    { get; set; }  // physician | nurse | tech | admin
    public string? EmployeeCode { get; set; }  // clinic's internal HR code
    public string? JoiningDate  { get; set; }  // yyyy-MM-dd

    // ── Status in this clinic ─────────────────────────────────────────────
    public string Status { get; set; } = "active"; // active | suspended | terminated

    // ── Extension ─────────────────────────────────────────────────────────
    public string ExtensionData { get; set; } = "{}";

    // ── Audit ─────────────────────────────────────────────────────────────
    public DateTime CreatedAt  { get; set; }
    public DateTime UpdatedAt  { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid?    CreatedBy  { get; set; }
}
