using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace LiPi.Web.Services;

/// <summary>
/// Email service using SMTP — Gmail for dev, SendGrid/SES for production.
/// Switch provider by changing appsettings Email:Smtp section only — no code change.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration         _config;
    private readonly ILogger<SmtpEmailService> _log;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> log)
    {
        _config = config;
        _log    = log;
    }

    public async Task SendOtpAsync(string toEmail, string toName, string otp, string purpose)
    {
        var subject = purpose == "forgot_password"
            ? "LiPi HIS — Password Reset OTP"
            : "LiPi HIS — Your Login OTP";

        var body = $@"
<div style=""font-family:Arial,sans-serif;max-width:480px;margin:0 auto"">
  <div style=""background:#0B2545;padding:20px 24px;border-radius:8px 8px 0 0"">
    <h2 style=""color:#fff;margin:0;font-size:20px"">LiPi HIS</h2>
    <p style=""color:#94A3B8;margin:4px 0 0;font-size:12px"">Armoki Healthcare Information System</p>
  </div>
  <div style=""background:#F8FAFC;padding:24px;border-radius:0 0 8px 8px;border:1px solid #E2E8F0"">
    <p style=""color:#0F2D5E;font-size:14px"">Hello {toName},</p>
    <p style=""color:#475569;font-size:13px"">Your one-time password (OTP) is:</p>
    <div style=""background:#fff;border:2px dashed #0B2545;border-radius:8px;padding:20px;text-align:center;margin:16px 0"">
      <span style=""font-size:36px;font-weight:700;letter-spacing:10px;color:#0B2545;font-family:monospace"">{otp}</span>
    </div>
    <p style=""color:#64748B;font-size:12px"">⏱ Valid for <strong>10 minutes</strong>. Do not share this with anyone.</p>
    <p style=""color:#94A3B8;font-size:11px;margin-top:20px;border-top:1px solid #E2E8F0;padding-top:12px"">
      If you did not request this, please ignore this email or contact your system administrator.
    </p>
  </div>
</div>";

        await SendAsync(toEmail, toName, subject, body);
    }

    public async Task SendPasswordChangedAsync(string toEmail, string toName)
    {
        var body = $@"
<div style=""font-family:Arial,sans-serif;max-width:480px;margin:0 auto"">
  <div style=""background:#0B2545;padding:20px 24px;border-radius:8px 8px 0 0"">
    <h2 style=""color:#fff;margin:0"">LiPi HIS — Password Changed</h2>
  </div>
  <div style=""background:#F8FAFC;padding:24px;border-radius:0 0 8px 8px;border:1px solid #E2E8F0"">
    <p style=""color:#0F2D5E"">Hello {toName},</p>
    <p style=""color:#475569;font-size:13px"">Your LiPi HIS password was changed successfully.</p>
    <p style=""color:#475569;font-size:13px"">If you did not make this change, contact your administrator immediately.</p>
  </div>
</div>";

        await SendAsync(toEmail, toName, "LiPi HIS — Password Changed", body);
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var cfg  = _config.GetSection("Email:Smtp");
        var host = cfg["Host"] ?? "smtp.gmail.com";
        var port = cfg.GetValue<int>("Port", 587);
        var user = cfg["Username"] ?? string.Empty;
        var pass = cfg["Password"] ?? string.Empty;
        var from = cfg["FromAddress"] ?? user;
        var name = cfg["FromName"] ?? "LiPi HIS";

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            _log.LogWarning("Email not configured — skipping send to {Email}", toEmail);
            _log.LogInformation("OTP email SKIPPED (no SMTP config). Subject: {Subject}", subject);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(name, from));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body    = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(user, pass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _log.LogInformation("Email sent to {Email} — {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }
}
