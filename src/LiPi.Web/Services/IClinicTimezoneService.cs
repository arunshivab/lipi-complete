// SPEC:    docs/00-COMPONENTS/01.5-DateTime.md (shipping in Batch 9d)
// PHASE:   2 Sub-step 2.4 — Date/Time component family (Batch 9d)
// AMEND:   docs/CHANGE-LOG.md A20 (pending)
//
// IClinicTimezoneService — clinic timezone resolution for the LiPi Date/Time
// component family.
//
// Why a service (not DateTimeOffset.Now / DateTime.Now)?
//
//   System clock returns the SERVER's wall clock time. In multi-tenant LiPi
//   deployments the server may live anywhere (Mumbai, Singapore, AWS-Frankfurt,
//   etc.) but the clinic's relevant "now" is always the CLINIC's local time.
//   Using DateTime.Now would silently drift across deployments and timezones.
//
//   This service abstracts the "what time is it for THIS clinic?" question so
//   components ask the right question and get clinic-context-aware answers.
//
// Phase 2.4 default impl: hardcoded "Asia/Kolkata" (IST, UTC+5:30, NO DST).
// All India deployments are in this timezone; current LiPi market is India-only.
//
// Future: when clinic-config schema lands (master.clinics.timezone_id column —
// see design package §7), implementation reads from current clinic context.
// Components don't change.
//
// India + DST: India does NOT observe Daylight Saving Time. The country-wide
// IST offset is a constant +05:30 year-round. This means LipiDateTimePicker
// (which composes DateOnly + TimeOnly to DateTimeOffset) has unambiguous
// semantics in India — no fall-back hour, no spring-forward hour. For
// timezones with DST, the composition has known ambiguity gaps; this is
// documented in CHANGE-LOG A20 as a v1.1 international-expansion concern.

using System;

namespace LiPi.Web.Services;

/// <summary>
/// Provides the current clinic's timezone for clinic-aware time operations.
/// Implementations should resolve from clinic context, NOT from system clock.
/// The default Phase 2.4 implementation hardcodes India ("Asia/Kolkata") until
/// the clinic-config schema lands.
/// </summary>
public interface IClinicTimezoneService
{
    /// <summary>
    /// Get the current clinic's <see cref="TimeZoneInfo"/>. Used for:
    ///   - Composing DateTimeOffset from DateOnly + TimeOnly (LipiDateTimePicker)
    ///   - Resolving "now" for clinic-local Now buttons (LipiTimePicker, LipiDateTimePicker)
    ///   - Converting UTC TIMESTAMPTZ values to clinic-local for display
    /// </summary>
    TimeZoneInfo GetClinicTimezone();

    /// <summary>
    /// Get the current "now" in the clinic's local timezone, as a
    /// <see cref="DateTime"/> with <c>Kind=Unspecified</c> (it represents a
    /// wall-clock time, not a UTC instant).
    ///
    /// Used by LipiTimePicker.NowButton and LipiDateTimePicker.NowButton to
    /// fill in the current moment.
    /// </summary>
    DateTime GetClinicLocalNow();

    /// <summary>
    /// Convert a UTC <see cref="DateTimeOffset"/> to the clinic's local
    /// timezone. Returns a new <see cref="DateTimeOffset"/> with the clinic's
    /// offset applied. The instant in time is preserved; only the offset
    /// representation changes.
    /// </summary>
    DateTimeOffset ToClinicLocal(DateTimeOffset utc);

    /// <summary>
    /// Convert a clinic-local wall-clock <see cref="DateTime"/> to a UTC
    /// <see cref="DateTimeOffset"/>. Used when persisting user-input clinic
    /// times to TIMESTAMPTZ columns.
    ///
    /// Caller is responsible for the wall-clock <see cref="DateTime"/> being
    /// unambiguous. For India (no DST) this is always the case. For DST-
    /// observing timezones, callers must handle the fall-back-hour ambiguity
    /// (deferred to v1.1 — see CHANGE-LOG).
    /// </summary>
    DateTimeOffset ToUtc(DateTime clinicLocal);
}
