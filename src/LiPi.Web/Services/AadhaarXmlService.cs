using System.IO.Compression;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace LiPi.Web.Services;

/// <summary>
/// Decrypts and parses UIDAI Offline Aadhaar eKYC XML.
/// The patient downloads a ZIP from uidai.gov.in and provides a 4-digit share code.
/// Full Aadhaar number is NEVER stored — only demographics extracted from XML.
/// HIPAA + DPDP Act compliance: requires explicit patient consent before processing.
/// </summary>
public interface IAadhaarXmlService
{
    Task<AadhaarResult> ParseAsync(Stream zipStream, string shareCode);
}

public sealed class AadhaarResult
{
    public bool   Success      { get; init; }
    public string? Error       { get; init; }

    // Demographics parsed from Aadhaar XML — store these, NOT the Aadhaar number
    public string? FullName    { get; init; }
    public string? DateOfBirth { get; init; }   // YYYY-MM-DD or YYYY (year-only)
    public string? Gender      { get; init; }   // "M" | "F" | "T"
    public string? AddressLine { get; init; }
    public string? District    { get; init; }
    public string? State       { get; init; }
    public string? PinCode     { get; init; }
    public string? PhotoBase64 { get; init; }   // JPEG base64 — store in blob, not DB column
    public string? LastFourUid { get; init; }   // Last 4 digits of UID only
}

public class AadhaarXmlService : IAadhaarXmlService
{
    private readonly ILogger<AadhaarXmlService> _log;

    public AadhaarXmlService(ILogger<AadhaarXmlService> log)
        => _log = log;

    public async Task<AadhaarResult> ParseAsync(Stream zipStream, string shareCode)
    {
        if (string.IsNullOrWhiteSpace(shareCode) || shareCode.Length != 4 || !shareCode.All(char.IsDigit))
            return Fail("Share code must be exactly 4 digits.");

        try
        {
            // Read ZIP into memory so we can pass it to SharpZipLib
            using var ms = new MemoryStream();
            await zipStream.CopyToAsync(ms);
            ms.Position = 0;

            // SharpZipLib handles password-protected ZIPs from UIDAI
            var zipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(ms);
            try
            {
                zipFile.Password = shareCode;

                // Find the XML entry inside the ZIP
                ZipEntry? xmlEntry = null;
                foreach (ZipEntry entry in zipFile)
                {
                    if (entry.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    { xmlEntry = entry; break; }
                }

                if (xmlEntry == null)
                    return Fail("No XML file found inside the ZIP. Please download Offline Aadhaar from uidai.gov.in.");

                using var xmlStream = zipFile.GetInputStream(xmlEntry);
                using var reader    = new StreamReader(xmlStream);
                var xmlContent      = await reader.ReadToEndAsync();

                return ParseXml(xmlContent);
            }
            finally { zipFile.Close(); }
        }
        catch (ZipException ex) when (ex.Message.Contains("password", StringComparison.OrdinalIgnoreCase))
        {
            return Fail("Incorrect share code. Please enter the 4-digit code you set when downloading.");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Aadhaar XML parsing failed");
            return Fail("Could not read the file. Ensure it is the ZIP downloaded from uidai.gov.in.");
        }
    }

    private static AadhaarResult ParseXml(string xml)
    {
        try
        {
            var doc  = XDocument.Parse(xml);
            var root = doc.Root; // <OfflinePaperlessKyc>

            // Last 4 digits of UID (from uid attribute, masked)
            var uid      = root?.Attribute("uid")?.Value ?? string.Empty;
            var last4    = uid.Length >= 4 ? uid[^4..] : string.Empty;

            var uidData  = root?.Element("UidData");
            var poi      = uidData?.Element("Poi"); // Proof of Identity
            var poa      = uidData?.Element("Poa"); // Proof of Address
            var pht      = uidData?.Element("Pht"); // Photo

            // Parse demographics from Poi
            var name     = poi?.Attribute("name")?.Value;
            var dob      = poi?.Attribute("dob")?.Value    // YYYY-MM-DD
                        ?? poi?.Attribute("yob")?.Value;   // year-only fallback
            var genderRaw= poi?.Attribute("gender")?.Value; // M / F / T

            // Parse address from Poa
            var house    = poa?.Attribute("house")?.Value  ?? string.Empty;
            var street   = poa?.Attribute("street")?.Value ?? string.Empty;
            var vtc      = poa?.Attribute("vtc")?.Value    ?? string.Empty; // village/town
            var po       = poa?.Attribute("po")?.Value     ?? string.Empty;
            var subdist  = poa?.Attribute("subdist")?.Value ?? string.Empty;
            var lm       = poa?.Attribute("lm")?.Value     ?? string.Empty; // landmark
            var dist     = poa?.Attribute("dist")?.Value   ?? string.Empty;
            var state    = poa?.Attribute("state")?.Value  ?? string.Empty;
            var pinCode  = poa?.Attribute("pc")?.Value     ?? string.Empty;
            var co       = poa?.Attribute("co")?.Value     ?? string.Empty; // care-of

            // Build address line from available parts
            var addrParts = new[] { co, house, street, vtc, po, lm, subdist }
                .Where(x => !string.IsNullOrWhiteSpace(x));
            var addressLine = string.Join(", ", addrParts);

            return new AadhaarResult
            {
                Success     = true,
                FullName    = name,
                DateOfBirth = dob,
                Gender      = genderRaw,
                AddressLine = addressLine,
                District    = dist,
                State       = state,
                PinCode     = pinCode,
                PhotoBase64 = pht?.Value,
                LastFourUid = last4,
            };
        }
        catch (Exception)
        {
            return Fail("Could not parse Aadhaar XML. The file may be corrupted or not a valid UIDAI document.");
        }
    }

    private static AadhaarResult Fail(string error) =>
        new() { Success = false, Error = error };
}
