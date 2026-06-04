// SPEC: docs/00-COMPONENTS/2.8/DATETIME-CAPABILITY-SPEC-LOCKED.md §2, §6.2
// PHASE: DateTime migration → LiPi.Components.Forms (CHANGE-LOG A54)
// COMPONENT: Date/Time picker family — time-source + zone resolution
//
// Ports the *mechanics* of the former LiPi.Web ClinicTimezoneService into the
// redistributable package as static helpers, driven by an explicit LipiTimeSource
// + TimeZoneInfo instead of clinic context. NO clinic/tenant concept lives here —
// the consuming app (e.g. a HIS) decides the source/zone and feeds it in.
//
// "now" sourcing is now EXPLICIT (never an accidental DateTime.Now inside a
// component). Server = NTP-synced server clock (SaaS-safe default); Utc; Client
// (resolved via the JS bridge by the picker, not here); SpecificZone (pinned).

using System;

namespace LiPi.Components.Forms;

/// <summary>
/// Whose "now" the Date/Time pickers use for Today / Tomorrow / Yesterday / Now and
/// relative presets. The developer selects; the components implement all options.
/// </summary>
public enum LipiTimeSource
{
    /// <summary>The .NET server wall clock (<see cref="DateTime.Now"/>/<see cref="DateTime.Today"/>).
    /// DEFAULT. SaaS-safe: trust the server (kept NTP-synced) over a client clock/zone that
    /// may be wrong or manually mis-set.</summary>
    Server,

    /// <summary>UTC now (<see cref="DateTime.UtcNow"/>). Zone-neutral anchor.</summary>
    Utc,

    /// <summary>The browser's local clock/zone, read via JS interop. Use when the value is
    /// about the user's own local day and the app trusts the client. Resolved by the picker
    /// through the JS bridge (lipiInput.getClientNow), not by <see cref="LipiTimeResolver"/>.</summary>
    Client,

    /// <summary>An explicit zone supplied via the picker's TimeZone parameter (e.g.
    /// <see cref="LipiTimeZones.IndiaIST"/>). A HIS pins this to the clinic zone so every
    /// viewer sees the clinic's "today" regardless of where they sit.</summary>
    SpecificZone
}

/// <summary>
/// Convenience <see cref="TimeZoneInfo"/> instances. Indian-company batteries-included.
/// </summary>
public static class LipiTimeZones
{
    private const string IndiaTzId = "Asia/Kolkata";
    private static readonly TimeSpan IndiaFixedOffset = new(5, 30, 0);  // +05:30, no DST

    // Ported verbatim from ClinicTimezoneService.ResolveIndianTimezone — IANA id with a
    // defensive fixed-offset fallback for environments without ICU (e.g. minimal containers).
    private static TimeZoneInfo ResolveIndia()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(IndiaTzId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.CreateCustomTimeZone(
                id: "IST",
                baseUtcOffset: IndiaFixedOffset,
                displayName: "(UTC+05:30) India Standard Time",
                standardDisplayName: "IST");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.CreateCustomTimeZone(
                id: "IST",
                baseUtcOffset: IndiaFixedOffset,
                displayName: "(UTC+05:30) India Standard Time",
                standardDisplayName: "IST");
        }
    }

    // Cached for process lifetime — TZ data doesn't change at runtime; TimeZoneInfo is thread-safe.
    private static readonly TimeZoneInfo _indiaIst = ResolveIndia();

    /// <summary>India Standard Time (Asia/Kolkata, UTC+05:30, no DST), with an ICU-less
    /// fixed-offset fallback. Use with <c>TimeSource=SpecificZone</c>.</summary>
    public static TimeZoneInfo IndiaIST => _indiaIst;
}

/// <summary>
/// Static time/zone resolution for the Date/Time picker family. Replaces the former
/// ClinicTimezoneService; all methods are parameterized by <see cref="LipiTimeSource"/> +
/// <see cref="TimeZoneInfo"/> rather than clinic context.
/// </summary>
internal static class LipiTimeResolver
{
    /// <summary>
    /// Resolve "now" as a wall-clock <see cref="DateTime"/> (<c>Kind=Unspecified</c> for the
    /// zone-bearing cases) per the chosen source. <see cref="LipiTimeSource.Client"/> is NOT
    /// handled here (it needs the browser) — the picker resolves Client via the JS bridge and
    /// does not call this with Client.
    /// </summary>
    public static DateTime ResolveNow(LipiTimeSource source, TimeZoneInfo? zone)
    {
        switch (source)
        {
            case LipiTimeSource.Utc:
                return DateTime.UtcNow;

            case LipiTimeSource.SpecificZone:
                // Generalized GetClinicLocalNow: UTC now → the specified zone's wall clock.
                var tz = zone ?? TimeZoneInfo.Utc;
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

            case LipiTimeSource.Client:
                // Should not reach here — Client is JS-resolved by the picker. Fall back to
                // server local rather than throw, so a mis-wired call degrades gracefully.
                return DateTime.Now;

            case LipiTimeSource.Server:
            default:
                return DateTime.Now;
        }
    }

    /// <summary>Resolve "today" as a <see cref="DateOnly"/> per the chosen source.</summary>
    public static DateOnly ResolveToday(LipiTimeSource source, TimeZoneInfo? zone)
        => DateOnly.FromDateTime(ResolveNow(source, zone));

    /// <summary>
    /// Convert a <see cref="DateTimeOffset"/> to the given zone's offset, preserving the
    /// instant (only the offset representation changes). Generalized from ToClinicLocal.
    /// Used for the DateTimePicker's DisplayZone.
    /// </summary>
    public static DateTimeOffset ToZone(DateTimeOffset value, TimeZoneInfo zone)
        => TimeZoneInfo.ConvertTime(value, zone);

    /// <summary>
    /// Compose a clinic-/zone-local wall-clock date+time into a <see cref="DateTimeOffset"/>
    /// using the given zone's offset for that wall-clock moment. Generalized from ToUtc:
    /// resolves the correct offset (for non-DST zones like India this is constant; for
    /// DST zones .NET picks the standard offset on ambiguous wall-clock times — v1.1 concern).
    /// </summary>
    public static DateTimeOffset Compose(DateOnly date, TimeOnly time, TimeZoneInfo zone)
    {
        var wall = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified);
        var offset = zone.GetUtcOffset(wall);
        return new DateTimeOffset(wall, offset);
    }
}
