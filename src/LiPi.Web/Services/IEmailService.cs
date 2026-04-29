namespace LiPi.Web.Services;

public interface IEmailService
{
    Task SendOtpAsync(string toEmail, string toName, string otp, string purpose);
    Task SendPasswordChangedAsync(string toEmail, string toName);
}
