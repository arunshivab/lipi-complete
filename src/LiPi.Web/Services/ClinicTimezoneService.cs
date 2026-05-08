// SPEC:    docs/00-COMPONENTS/01.5-DateTime.md (shipping in Batch 9d)
// PHASE:   2 Sub-step 2.4 — Date/Time component family (Batch 9d)
// AMEND:   docs/CHANGE-LOG.md A20 (pending)
//
// ClinicTimezoneService — Phase 2.4 default implementation.
//
// Hardcodes "Asia/Kolkata" (IST, UTC+5:30) for India. When clinic-config
// schema lands (master.clinics.timezone_id column), implementation will
// read clinic context.
//
// IANA TZ ID compatibility:
//   - Linux/macOS: ICU TZ database, "Asia/Kolkata" works directly
//   - Windows: until .NET 6 used Windows TZ IDs ("India Standard Time").
//     .NET 6+ accepts BOTH IANA and Windows IDs via TimeZoneInfo.FindSystemTimeZoneById
//   - We use the IANA "Asia/Kolkata" since LiPi targets .NET 10 (.NET 6+ semantics)
//   - Defensive fallback to UTC+5:30 fixed offset if FindSystemTimeZoneById fails
//     (e.g., minimal ICU install on a container)

using System;

namespace LiPi.Web.Services;

public sealed class ClinicTimezoneService : IClinicTimezoneService
{
    // ==========================================================================
    // CLINIC TIMEZONE — Phase 2.4 hardcodes India until clinic-config lands
    // ==========================================================================

    private const string IndianTimezoneId = "Asia/Kolkata";
    private static readonly TimeSpan IndianOffset = new(5, 30, 0);  // +05:30, no DST

    private static TimeZoneInfo ResolveIndianTimezone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(IndianTimezoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            // Fallback for environments without ICU — use a fixed offset.
            // This is functionally identical for India (no DST), but loses
            // the "Asia/Kolkata" display name.
            return TimeZoneInfo.CreateCustomTimeZone(
                id: "IST",
                baseUtcOffset: IndianOffset,
                displayName: "(UTC+05:30) India Standard Time",
                standardDisplayName: "IST");
        }
        catch (InvalidTimeZoneException)
        {
            // Same fallback — corrupted TZ data
            return TimeZoneInfo.CreateCustomTimeZone(
                id: "IST",
                baseUtcOffset: IndianOffset,
                displayName: "(UTC+05:30) India Standard Time",
                standardDisplayName: "IST");
        }
    }

    // Cache the resolved TimeZoneInfo for the process lifetime — TZ data doesn't
    // change at runtime, and TimeZoneInfo objects are thread-safe per .NET docs.
    private static readonly TimeZoneInfo ClinicTz = ResolveIndianTimezone();

    public TimeZoneInfo GetClinicTimezone() => ClinicTz;

    public DateTime GetClinicLocalNow()
    {
        // Convert UTC now to clinic-local. Result is Kind=Unspecified per
        // TimeZoneInfo.ConvertTimeFromUtc semantics — represents wall-clock
        // time, not a UTC instant. This is the correct shape for "what time
        // is it on the clinic wall clock?".
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ClinicTz);
    }

    public DateTimeOffset ToClinicLocal(DateTimeOffset utc)
    {
        // Preserves the instant; only changes the offset representation.
        return TimeZoneInfo.ConvertTime(utc, ClinicTz);
    }

    public DateTimeOffset ToUtc(DateTime clinicLocal)
    {
        // Caller's DateTime is presumed to be a clinic-local wall-clock time.
        // We DateTimeKind.Unspecified-it explicitly so TimeZoneInfo doesn't
        // misinterpret a Local-kinded DateTime as the SERVER's local time.
        var unspecified = DateTime.SpecifyKind(clinicLocal, DateTimeKind.Unspecified);

        // Get the offset for this specific wall-clock time. For India (no DST)
        // this is always +05:30. For DST-observing zones, this would resolve
        // to either standard or daylight offset based on the wall-clock time.
        // Ambiguous wall-clock times (fall-back hour) get the STANDARD offset
        // by .NET convention — see GetUtcOffset docs. India never hits this.
        var offset = ClinicTz.GetUtcOffset(unspecified);

        return new DateTimeOffset(unspecified, offset);
    }
}
