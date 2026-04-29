using LiPi.Clinic.Core;
using LiPi.Clinic.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiPi.Web.Services;

// ── Request / Result models ────────────────────────────────────────────────

public sealed class DuplicateCheckRequest
{
    public string? AadhaarLastFour { get; init; }
    public string? AbhaNumber      { get; init; }
    public string  GivenName       { get; init; } = string.Empty;
    public string? FamilyName      { get; init; }
    public string? DateOfBirth     { get; init; }  // yyyy-MM-dd or null
    public string? AgeDisplay      { get; init; }  // numeric string if DOB unknown
    public string  DobConfidence   { get; init; } = "estimated"; // verified|self_reported|estimated|unknown
    public string? Mobile          { get; init; }  // digits only
}

public sealed class DuplicateMatch
{
    public Guid        PatientId      { get; init; }
    public string      Mrn            { get; init; } = string.Empty;
    public string      DisplayName    { get; init; } = string.Empty;
    public string?     DateOfBirth    { get; init; }
    public string?     DobConfidence  { get; init; }
    public string?     Mobile         { get; init; }
    public string?     AadhaarLastFour{ get; init; }
    public string      MatchReason    { get; init; } = string.Empty;  // e.g. "aadhaar|name_dob_mobile"
    public string      MatchStrength  { get; init; } = string.Empty;  // high|probable|possible
    public List<string>MatchedFields  { get; init; } = new();
}

// ── Interface ──────────────────────────────────────────────────────────────

public interface IDuplicateDetectionService
{
    Task<List<DuplicateMatch>> CheckAsync(
        Guid                   clinicId,
        DuplicateCheckRequest  request,
        ClinicCoreDbContext     db);
}

// ── Implementation ─────────────────────────────────────────────────────────

public sealed class DuplicateDetectionService : IDuplicateDetectionService
{
    private readonly ILogger<DuplicateDetectionService> _log;

    public DuplicateDetectionService(ILogger<DuplicateDetectionService> log)
        => _log = log;

    public async Task<List<DuplicateMatch>> CheckAsync(
        Guid                  clinicId,
        DuplicateCheckRequest req,
        ClinicCoreDbContext    db)
    {
        var results = new List<DuplicateMatch>();

        try
        {
            // Base query: active patients in this clinic with their identifiers
            var baseQ = db.Patients
                .Where(p => p.ClinicId == clinicId && p.Person.DeletedAt == null)
                .Include(p => p.Person)
                    .ThenInclude(pe => pe.ContactPoints)
                .Include(p => p.Identifiers)
                .AsNoTracking();

            // ── STEP 1: Aadhaar last-4 exact match ─────────────────────────
            if (!string.IsNullOrWhiteSpace(req.AadhaarLastFour))
            {
                var aadhaarMatches = await baseQ
                    .Where(p => p.Identifiers.Any(id =>
                        id.IdentifierType == "aadhaar" &&
                        id.IdentifierValue.EndsWith(req.AadhaarLastFour)))
                    .ToListAsync();

                foreach (var p in aadhaarMatches)
                {
                    if (results.Any(r => r.PatientId == p.Id)) continue;
                    results.Add(BuildMatch(p, "aadhaar", "high",
                        new[] { "Aadhaar last-4 exact match" }));
                }
            }

            // ── STEP 2: ABHA number exact match ────────────────────────────
            if (!string.IsNullOrWhiteSpace(req.AbhaNumber))
            {
                var abhaMatches = await baseQ
                    .Where(p => p.Identifiers.Any(id =>
                        id.IdentifierType == "abha_number" &&
                        id.IdentifierValue == req.AbhaNumber))
                    .ToListAsync();

                foreach (var p in abhaMatches)
                {
                    if (results.Any(r => r.PatientId == p.Id)) continue;
                    results.Add(BuildMatch(p, "abha", "high",
                        new[] { "ABHA number exact match" }));
                }
            }

            // ── STEP 3: Soundex name + DOB window ──────────────────────────
            var givenSoundex  = Soundex(req.GivenName);
            var familySoundex = Soundex(req.FamilyName ?? string.Empty);

            // Pull candidate patients for in-memory matching (name-based pre-filter)
            // We filter by first letter of given name to reduce the candidate set
            var firstLetter = req.GivenName.Length > 0
                ? req.GivenName[0].ToString().ToUpperInvariant()
                : string.Empty;

            var nameCandidates = await baseQ
                .Where(p => p.Person.GivenName.StartsWith(firstLetter))
                .ToListAsync();

            foreach (var p in nameCandidates)
            {
                if (results.Any(r => r.PatientId == p.Id)) continue;

                var pGivenSx  = Soundex(p.Person.GivenName);
                var pFamilySx = Soundex(p.Person.FamilyName ?? string.Empty);

                var nameMatch = pGivenSx == givenSoundex &&
                                (string.IsNullOrEmpty(familySoundex) ||
                                 string.IsNullOrEmpty(pFamilySx)     ||
                                 pFamilySx == familySoundex);

                if (!nameMatch) continue;

                // DOB window check
                var dobMatch = DobWindowMatch(
                    req.DateOfBirth, req.DobConfidence,
                    p.Person.DateOfBirth?.ToString("yyyy-MM-dd"),
                    "self_reported");  // existing records confidence assumed self-reported unless verified

                var matchedFields = new List<string>();
                if (nameMatch) matchedFields.Add("Name (Soundex)");

                if (!dobMatch && !string.IsNullOrEmpty(req.DateOfBirth))
                    continue;  // name matches but DOB is outside window

                if (dobMatch) matchedFields.Add("Date of birth");

                // Also check mobile
                var mobile = p.Person.ContactPoints
                    .FirstOrDefault(cp => cp.System == "phone" && cp.IsPrimary)?.Value;
                var mobileDigits = new string((mobile ?? string.Empty).Where(char.IsDigit).TakeLast(10).ToArray());
                var reqMobile    = new string((req.Mobile ?? string.Empty).Where(char.IsDigit).TakeLast(10).ToArray());
                var mobileMatch  = !string.IsNullOrEmpty(reqMobile) &&
                                   !string.IsNullOrEmpty(mobileDigits) &&
                                   mobileDigits == reqMobile;

                if (mobileMatch) matchedFields.Add("Mobile number");

                var strength = mobileMatch ? "probable" : "possible";
                var reason   = mobileMatch ? "name_dob_mobile" : "name_dob";

                results.Add(BuildMatch(p, reason, strength, matchedFields));
            }

            // Sort: high first, then probable, then possible
            results = results
                .OrderBy(r => r.MatchStrength switch { "high" => 0, "probable" => 1, _ => 2 })
                .Take(10)
                .ToList();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Duplicate check failed for clinic {ClinicId}", clinicId);
        }

        return results;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static DuplicateMatch BuildMatch(
        Patient        p,
        string         reason,
        string         strength,
        IEnumerable<string> fields)
    {
        var mobile = p.Person.ContactPoints
            .FirstOrDefault(cp => cp.System == "phone" && cp.IsPrimary)?.Value;

        var aadhaar = p.Identifiers
            .FirstOrDefault(id => id.IdentifierType == "aadhaar")?.IdentifierValue;

        return new DuplicateMatch
        {
            PatientId       = p.Id,
            Mrn             = p.Mrn,
            DisplayName     = p.Person.DisplayName,
            DateOfBirth     = p.Person.DateOfBirth?.ToString("yyyy-MM-dd"),
            DobConfidence   = "self_reported",
            Mobile          = mobile,
            AadhaarLastFour = aadhaar != null && aadhaar.Length >= 4
                              ? aadhaar[^4..] : aadhaar,
            MatchReason     = reason,
            MatchStrength   = strength,
            MatchedFields   = fields.ToList(),
        };
    }

    private static bool DobWindowMatch(
        string? reqDob, string reqConf,
        string? existDob, string existConf)
    {
        if (string.IsNullOrEmpty(reqDob) || string.IsNullOrEmpty(existDob))
            return true;  // one side unknown — don't use DOB as a disqualifier

        if (!DateOnly.TryParse(reqDob,   out var d1)) return true;
        if (!DateOnly.TryParse(existDob, out var d2)) return true;

        // Window based on the LESS confident of the two sides
        var window = (reqConf, existConf) switch
        {
            ("verified",      _)            => 0,
            (_,               "verified")   => 0,
            ("self_reported", _)            => 1,
            (_,               "self_reported") => 1,
            ("estimated",     _)            => 3,
            _                               => 5,
        };

        return Math.Abs(d1.DayNumber - d2.DayNumber) <= window * 366;
    }

    /// <summary>American Metaphone-lite Soundex, adequate for Indian names.</summary>
    public static string Soundex(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "0000";

        s = s.ToUpperInvariant();
        var sb = new System.Text.StringBuilder();
        sb.Append(s[0]);

        static char Map(char c) => c switch
        {
            'B' or 'F' or 'P' or 'V'                         => '1',
            'C' or 'G' or 'J' or 'K' or 'Q' or 'S' or 'X' or 'Z' => '2',
            'D' or 'T'                                        => '3',
            'L'                                               => '4',
            'M' or 'N'                                        => '5',
            'R'                                               => '6',
            _                                                 => '0',
        };

        var prev = Map(s[0]);
        for (var i = 1; i < s.Length && sb.Length < 4; i++)
        {
            var code = Map(s[i]);
            if (code == '0') { prev = '0'; continue; }
            if (code == prev) continue;
            sb.Append(code);
            prev = code;
        }

        while (sb.Length < 4) sb.Append('0');
        return sb.ToString();
    }
}
