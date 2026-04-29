using System.Security.Cryptography;
using System.Text;
using LiPi.Master;
using Microsoft.EntityFrameworkCore;

namespace LiPi.Web.Services;

/// <summary>
/// Encrypts / decrypts per-clinic DB connection strings using AES-256-GCM.
/// Key is stored in config: LiPi:EncryptionKey (base64, 32 bytes).
/// In production use Azure Key Vault. In dev use appsettings.Development.json.
/// </summary>
public class ClinicConnectionService
{
    private readonly IDbContextFactory<MasterDbContext> _masterFactory;
    private readonly ILogger<ClinicConnectionService>   _log;
    private readonly byte[]                             _key;

    // Simple in-memory cache: clinicId → decrypted connection string
    private readonly Dictionary<Guid, string> _cache = new();

    public ClinicConnectionService(
        IDbContextFactory<MasterDbContext> masterFactory,
        ILogger<ClinicConnectionService>   log,
        IConfiguration                     config)
    {
        _masterFactory = masterFactory;
        _log           = log;

        var keyB64 = config["LiPi:EncryptionKey"]
            ?? Convert.ToBase64String(Encoding.UTF8.GetBytes("LiPiDevKey12345678901234567890Ab")); // 32-byte dev fallback
        _key = Convert.FromBase64String(keyB64);
        if (_key.Length != 32)
            throw new InvalidOperationException("LiPi:EncryptionKey must be 32 bytes (256 bits) base64-encoded.");
    }

    // ── Encryption ────────────────────────────────────────────────────────────

    public string Encrypt(string plaintext)
    {
        var nonce      = new byte[AesGcm.NonceByteSizes.MinSize]; // 12 bytes
        var tag        = new byte[AesGcm.TagByteSizes.MaxSize];   // 16 bytes
        var ciphertext = new byte[Encoding.UTF8.GetByteCount(plaintext)];

        RandomNumberGenerator.Fill(nonce);

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aes.Encrypt(nonce, Encoding.UTF8.GetBytes(plaintext), ciphertext, tag);

        // Format: base64(nonce) + "." + base64(tag) + "." + base64(ciphertext)
        return $"{Convert.ToBase64String(nonce)}.{Convert.ToBase64String(tag)}.{Convert.ToBase64String(ciphertext)}";
    }

    public string Decrypt(string encrypted)
    {
        var parts = encrypted.Split('.');
        if (parts.Length != 3) throw new FormatException("Invalid encrypted connection string format.");

        var nonce      = Convert.FromBase64String(parts[0]);
        var tag        = Convert.FromBase64String(parts[1]);
        var ciphertext = Convert.FromBase64String(parts[2]);
        var plaintext  = new byte[ciphertext.Length];

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    // ── Connection string resolution ──────────────────────────────────────────

    /// <summary>
    /// Returns the decrypted connection string for a clinic.
    /// Falls back to building one from host/port/dbname if no encrypted string stored.
    /// </summary>
    public async Task<string?> GetConnectionStringAsync(Guid clinicId)
    {
        if (_cache.TryGetValue(clinicId, out var cached)) return cached;

        try
        {
            await using var db = await _masterFactory.CreateDbContextAsync();
            var clinicDb = await db.ClinicDatabases
                .Where(c => c.ClinicId == clinicId)
                .FirstOrDefaultAsync();

            if (clinicDb == null) return null;

            string connStr;
            if (!string.IsNullOrEmpty(clinicDb.EncryptedConnectionString))
            {
                connStr = Decrypt(clinicDb.EncryptedConnectionString);
            }
            else
            {
                // Build from parts — dev mode fallback
                var dbUser = clinicDb.DbUsername ?? "postgres";
                var dbPass = !string.IsNullOrEmpty(clinicDb.EncryptedDbPassword)
                    ? Decrypt(clinicDb.EncryptedDbPassword) : "postgres";
                connStr = $"Host={clinicDb.DbHost};Port={clinicDb.DbPort};" +
                          $"Database={clinicDb.DbName};Username={dbUser};Password={dbPass};";
            }

            _cache[clinicId] = connStr;
            return connStr;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to resolve connection string for clinic {ClinicId}", clinicId);
            return null;
        }
    }

    /// <summary>
    /// Stores a new encrypted connection string for a clinic.
    /// Call this when creating/updating a clinic's DB record.
    /// </summary>
    public async Task SetConnectionStringAsync(Guid clinicId, string connectionString)
    {
        await using var db = await _masterFactory.CreateDbContextAsync();
        var clinicDb = await db.ClinicDatabases.FirstOrDefaultAsync(c => c.ClinicId == clinicId);
        if (clinicDb == null) throw new InvalidOperationException($"No ClinicDatabase record for {clinicId}");

        clinicDb.EncryptedConnectionString = Encrypt(connectionString);
        clinicDb.UpdatedAt = DateTimeOffset.UtcNow;
        _cache.Remove(clinicId); // invalidate cache
        await db.SaveChangesAsync();
    }

    /// <summary>Invalidate cache for a clinic — call after connection string changes.</summary>
    public void InvalidateCache(Guid clinicId) => _cache.Remove(clinicId);
}
