namespace LiPi.Web.Services;

/// <summary>Result of an authentication attempt.</summary>
public record AuthResult(
    bool     Success,
    string?  Error,
    Guid?    UserId,
    string?  Username,
    string?  DisplayName,
    string?  StaffType,
    string?  ClinicCode,
    string[] Roles,
    Guid?    ClinicId      = null,
    string   AdminLevel    = "staff",
    bool     IsGlobalUser        = false,
    bool     MustChangePassword = false,
    List<ClinicAccess>? ClinicChoices = null
);

public interface IAuthService
{
    Task<AuthResult> SignInAsync(
        string  username,
        string  password,
        string? clinicCode = null,
        string? clientIp   = null,
        string? userAgent  = null);

    Task<AuthResult> SignInWithClinicAsync(
        string  username,
        string  password,
        Guid    clinicId,
        string? clientIp  = null,
        string? userAgent = null);
}
