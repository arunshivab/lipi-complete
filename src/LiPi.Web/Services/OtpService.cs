using Microsoft.Extensions.Caching.Memory;

namespace LiPi.Web.Services;

/// <summary>
/// Generates, stores and verifies OTPs using in-memory cache.
/// For production: swap to Redis or a DB-backed store for multi-instance deployments.
/// OTP is 6 digits, valid for 10 minutes, single-use.
/// </summary>
public class OtpService
{
    private readonly IMemoryCache           _cache;
    private readonly ILogger<OtpService>    _log;

    private static readonly TimeSpan Expiry = TimeSpan.FromMinutes(10);

    public OtpService(IMemoryCache cache, ILogger<OtpService> log)
    {
        _cache = cache;
        _log   = log;
    }

    /// <summary>Generate and store a 6-digit OTP for the given user ID.</summary>
    public string Generate(Guid userId, string purpose)
    {
        var otp = Random.Shared.Next(100_000, 999_999).ToString();
        var key = CacheKey(userId, purpose);
        _cache.Set(key, otp, Expiry);
        _log.LogInformation("OTP generated for user {UserId} purpose={Purpose}", userId, purpose);
        return otp;
    }

    /// <summary>Verify and consume an OTP. Returns true only once — OTP is deleted on success.</summary>
    public bool Verify(Guid userId, string purpose, string inputOtp)
    {
        var key = CacheKey(userId, purpose);
        if (!_cache.TryGetValue(key, out string? stored))
        {
            _log.LogWarning("OTP not found or expired for user {UserId}", userId);
            return false;
        }
        if (stored != inputOtp.Trim())
        {
            _log.LogWarning("OTP mismatch for user {UserId}", userId);
            return false;
        }
        _cache.Remove(key); // single-use
        _log.LogInformation("OTP verified for user {UserId}", userId);
        return true;
    }

    /// <summary>Store a reset token after OTP verified, before password is set.</summary>
    public string GenerateResetToken(Guid userId)
    {
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "-").Replace("/", "_").Replace("=", "");
        _cache.Set($"reset:{token}", userId, TimeSpan.FromMinutes(15));
        return token;
    }

    /// <summary>Validate and consume a reset token. Returns userId if valid.</summary>
    public Guid? ConsumeResetToken(string token)
    {
        var key = $"reset:{token}";
        if (!_cache.TryGetValue(key, out Guid userId)) return null;
        _cache.Remove(key);
        return userId;
    }

    private static string CacheKey(Guid userId, string purpose) =>
        $"otp:{userId}:{purpose}";
}
