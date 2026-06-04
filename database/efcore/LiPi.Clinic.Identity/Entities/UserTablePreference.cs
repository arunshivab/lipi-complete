// SPEC: docs/00-COMPONENTS/2.8/00-Phase2.8-Overview.md §2.5 (DB schema)
// CROSS-REF: CHANGE-LOG.md A38 (per-clinic persistence), A39 (Core→Identity move)
// PHASE: 2.8 Data Display — Stage 1C (entity relocated from LiPi.Clinic.Core)
//
// EF Core entity for identity.user_table_preferences. Lives in LiPi.Clinic.Identity
// alongside UserPreference (theme prefs, Decision #12) — its direct sibling. Both are
// per-user, per-clinic-DB preference rows in the identity schema of EACH CLINIC DB.
//
// Plain POCO by design: IdentityDbContext applies schema (HasDefaultSchema "identity"),
// snake_case column naming (its OnModelCreating loop), and all fluent config. This
// matches how UserPreference and every other entity in this context is shaped — no
// data annotations on the entity itself.
//
// Column mapping produced by IdentityDbContext's snake_case loop:
//   UserId    → user_id      (Guid,   part of composite PK)
//   TableId   → table_id     (varchar(200), part of composite PK)
//   PrefsJson → prefs_json   (jsonb)
//   UpdatedAt → updated_at   (timestamptz; app sets UTC on every write)
//
// Cross-DB FK note: user_id references master.platform_users(id) but PostgreSQL cannot
// enforce the constraint cross-DB. App-layer only. Safe because LiPi never hard-deletes
// users — clinic-user deletion is access revocation, so orphan rows never accumulate.

using System;

namespace LiPi.Clinic.Identity.Entities;

/// <summary>
/// Per-user-per-table preference row for LipiTable (Phase 2.8). Composite PK
/// (UserId, TableId). PrefsJson holds the serialized
/// LiPi.Components.DataDisplay.TablePreferences shape. One row per (user, table).
/// </summary>
public class UserTablePreference
{
    /// <summary>master.platform_users.id — app-layer enforced (no cross-DB FK).</summary>
    public Guid UserId { get; set; }

    /// <summary>TableId parameter of LipiTable (developer-chosen stable identifier).</summary>
    public string TableId { get; set; } = string.Empty;

    /// <summary>Serialized TablePreferences JSON (jsonb column).</summary>
    public string PrefsJson { get; set; } = "{}";

    /// <summary>Last write timestamp (UTC). Set by the app on every write.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
