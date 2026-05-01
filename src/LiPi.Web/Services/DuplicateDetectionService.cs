using LiPi.Clinic.Core;
using LiPi.Clinic.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiPi.Web.Services;

public sealed class DuplicateCheckRequest
{
    public string? AadhaarLastFour { get; init; }
    public string? AbhaNumber      { get; init; }
    public string  FirstName       { get; init; } = string.Empty;
    public string? LastName        { get; init; }
    public string? DateOfBirth     { get; init; }   // yyyy-MM-dd
    public string  DobConfidence   { get; init; } = "estimated";
    public string? Mobile          { get; init; }   // digits only
}

public sealed class DuplicateMatch
{
    public Guid         EntityId       { get; init; }  // stable patient entity UUID
    public string       Uhid           { get; init; } = string.Empty;
    public string       DisplayName    { get; init; } = string.Empty;
    public string?      DateOfBirth    { get; init; }
    public string?      DobConfidence  { get; init; }
    public string?      Mobile         { get; init; }
    public string?      AadhaarLastFour{ get; init; }
    public string       MatchReason    { get; init; } = string.Empty;
    public string       MatchStrength  { get; init; } = string.Empty;
    public List<string> MatchedFields  { get; init; } = new();
}

public interface IDuplicateDetectionService
{
    Task<List<DuplicateMatch>> CheckAsync(
        Guid                  clinicId,
        DuplicateCheckRequest request,
        ClinicCoreDbContext    db);
}

public sealed class DuplicateDetectionService : IDuplicateDetectionService
{
    private readonly ILogger<DuplicateDetectionService> _log;
    public DuplicateDetectionService(ILogger<DuplicateDetectionService> log) => _log = log;

    public async Task<List<DuplicateMatch>> CheckAsync(
        Guid                  clinicId,
        DuplicateCheckRequest req,
        ClinicCoreDbContext    db)
    {
        var results = new List<DuplicateMatch>();
        try
        {
            var baseQ = db.Patients
                .Where(p => p.ClinicId == clinicId)
                .Include(p => p.ContactPoints)
                .Include(p => p.Identifiers)
                .AsNoTracking();

            // ── STEP 1: Aadhaar last-4 exact match ─────────────────────
            if (!string.IsNullOrWhiteSpace(req.AadhaarLastFour))
            {
                var matches = await baseQ
                    .Where(p => p.Identifiers.Any(id =>
                        id.IdentifierType == "aadhaar" &&
                        id.IdentifierValue.EndsWith(req.AadhaarLastFour)))
                    .ToListAsync();
                foreach (var p in matches)
                {
                    if (results.Any(r => r.EntityId == p.EntityId)) continue;
                    results.Add(BuildMatch(p, "aadhaar", "high", ["Aadhaar last-4 exact match"]));
                }
            }

            // ── STEP 2: ABHA number exact match ────────────────────────
            if (!string.IsNullOrWhiteSpace(req.AbhaNumber))
            {
                var matches = await baseQ
                    .Where(p => p.Identifiers.Any(id =>
                        id.IdentifierType == "abha_number" &&
                        id.IdentifierValue == req.AbhaNumber))
                    .ToListAsync();
                foreach (var p in matches)
                {
                    if (results.Any(r => r.EntityId == p.EntityId)) continue;
                    results.Add(BuildMatch(p, "abha", "high", ["ABHA number exact match"]));
                }
            }

            // ── STEP 3: Soundex name + DOB + mobile ────────────────────
            var givenSx  = Soundex(req.FirstName);
            var familySx = Soundex(req.LastName ?? string.Empty);
            var firstLetter = req.FirstName.Length > 0
                ? req.FirstName[0].ToString().ToUpperInvariant() : string.Empty;

            var candidates = await baseQ
                .Where(p => p.FirstName.StartsWith(firstLetter))
                .ToListAsync();

            foreach (var p in candidates)
            {
                if (results.Any(r => r.EntityId == p.EntityId)) continue;

                var nameMatch =
                    Soundex(p.FirstName) == givenSx &&
                    (string.IsNullOrEmpty(familySx) ||
                     string.IsNullOrEmpty(Soundex(p.LastName)) ||
                     Soundex(p.LastName) == familySx);

                if (!nameMatch) continue;

                var dobMatch = DobWindowMatch(
                    req.DateOfBirth, req.DobConfidence,
                    p.DateOfBirth.ToString("yyyy-MM-dd"), p.DobConfidence);

                if (!dobMatch && !string.IsNullOrEmpty(req.DateOfBirth)) continue;

                var matchedFields = new List<string> { "Name (Soundex)" };
                if (dobMatch) matchedFields.Add("Date of birth");

                var primaryPhone = p.ContactPoints
                    .FirstOrDefault(cp => cp.System == "phone" && cp.IsPrimary)?.Value;
                var existDigits = new string((primaryPhone ?? "").Where(char.IsDigit).TakeLast(10).ToArray());
                var reqDigits   = new string((req.Mobile ?? "").Where(char.IsDigit).TakeLast(10).ToArray());
                var mobileMatch = !string.IsNullOrEmpty(reqDigits) &&
                                  !string.IsNullOrEmpty(existDigits) &&
                                  existDigits == reqDigits;
                if (mobileMatch) matchedFields.Add("Mobile number");

                results.Add(BuildMatch(p,
                    mobileMatch ? "name_dob_mobile" : "name_dob",
                    mobileMatch ? "probable" : "possible",
                    matchedFields));
            }

            results = results
                .OrderBy(r => r.MatchStrength switch { "high" => 0, "probable" => 1, _ => 2 })
                .Take(10).ToList();
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Duplicate check failed for clinic {ClinicId}", clinicId);
        }
        return results;
    }

    private static DuplicateMatch BuildMatch(
        Patient p, string reason, string strength, IEnumerable<string> fields)
    {
        var mobile  = p.ContactPoints.FirstOrDefault(cp => cp.System == "phone" && cp.IsPrimary)?.Value;
        var aadhaar = p.Identifiers.FirstOrDefault(id => id.IdentifierType == "aadhaar")?.IdentifierValue;
        return new DuplicateMatch
        {
            EntityId        = p.EntityId,
            Uhid            = p.Uhid,
            DisplayName     = p.DisplayName,
            DateOfBirth     = p.DateOfBirth.ToString("yyyy-MM-dd"),
            DobConfidence   = p.DobConfidence,
            Mobile          = mobile,
            AadhaarLastFour = aadhaar != null && aadhaar.Length >= 4 ? aadhaar[^4..] : aadhaar,
            MatchReason     = reason,
            MatchStrength   = strength,
            MatchedFields   = fields.ToList(),
        };
    }

    private static bool DobWindowMatch(
        string? reqDob, string reqConf, string? existDob, string existConf)
    {
        if (string.IsNullOrEmpty(reqDob) || string.IsNullOrEmpty(existDob)) return true;
        if (!DateOnly.TryParse(reqDob,   out var d1)) return true;
        if (!DateOnly.TryParse(existDob, out var d2)) return true;
        var window = (reqConf, existConf) switch
        {
            ("verified",      _)              => 0,
            (_,               "verified")     => 0,
            ("self_reported", _)              => 1,
            (_,               "self_reported")=> 1,
            ("estimated",     _)              => 3,
            _                                 => 5,
        };
        return Math.Abs(d1.DayNumber - d2.DayNumber) <= window * 366;
    }

    public static string Soundex(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "0000";
        s = s.ToUpperInvariant();
        var sb = new System.Text.StringBuilder();
        sb.Append(s[0]);
        static char Map(char c) => c switch
        {
            'B' or 'F' or 'P' or 'V'                              => '1',
            'C' or 'G' or 'J' or 'K' or 'Q' or 'S' or 'X' or 'Z'=> '2',
            'D' or 'T'                                             => '3',
            'L'                                                    => '4',
            'M' or 'N'                                             => '5',
            'R'                                                    => '6',
            _                                                      => '0',
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
