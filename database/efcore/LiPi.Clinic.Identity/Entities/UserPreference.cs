// SPEC:     docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md §Database Schema Additions
// DECISION: docs/00-PROJECT-BASELINE.md §12.4 (Multi-Theme System)
// Phase:    Phase 1 — Theming Architecture, Deliverable 4
//
// Maps to identity.user_preferences in each clinic's identity database.
// Logical FK to master.platform_users.user_id (cross-DB; no physical constraint).

namespace LiPi.Clinic.Identity.Entities;

public class UserPreference
{
    public Guid     UserId    { get; set; }
    public string   ThemeMode { get; set; } = "light";     // light | dark | auto | high-contrast
    public string   Density   { get; set; } = "compact";   // comfortable | compact | spacious
    public string   FontSize  { get; set; } = "standard";  // standard | larger
    public string   Language  { get; set; } = "en";        // BCP 47 (e.g. "en", "hi", "en-IN")
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
