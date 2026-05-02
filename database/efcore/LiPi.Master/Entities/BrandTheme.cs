// SPEC:     docs/00-COMPONENTS/00.2-THEMING-ARCHITECTURE.md §Database Schema Additions
// DECISION: docs/00-PROJECT-BASELINE.md §12.4 (Multi-Theme System)
// Phase:    Phase 1 — Theming Architecture, Deliverable 4
//
// Maps to master.brand_themes table (created by 2026-05-02-decision-12-theming-up.sql).
// Adding a new client brand = INSERT row + add CSS file. No code change required.

namespace LiPi.Master.Entities;

public class BrandTheme
{
    public string    BrandId            { get; set; } = string.Empty;
    public string    DisplayName        { get; set; } = string.Empty;
    public string?   Description        { get; set; }
    public string    CssFilePath        { get; set; } = string.Empty;  // relative to wwwroot/
    public string?   LogoLightUrl       { get; set; }
    public string?   LogoDarkUrl        { get; set; }
    public bool      IsActive           { get; set; } = true;
    public bool      IsDeprecated       { get; set; } = false;
    public DateTime? DeprecatedAt       { get; set; }
    public string?   DeprecationReason  { get; set; }
    public int       SortOrder          { get; set; } = 100;
    public DateTime  CreatedAt          { get; set; }
    public DateTime  UpdatedAt          { get; set; }
}
