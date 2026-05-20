using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using VehiclePartsSystem.Application.Common.Interfaces;

namespace VehiclePartsSystem.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config) => _config = config;

    public async Task SendAsync(string toEmail, string subject, string htmlBody, byte[]? attachment = null, string? attachmentName = null)
    {
        var smtp = _config.GetSection("Smtp");
        var host = smtp["Host"];
        var portStr = smtp["Port"];
        var username = smtp["Username"];
        var password = smtp["Password"];
        var fromAddress = smtp["FromAddress"];

        // Fail fast with a clear message if SMTP is missing or still on the
        // appsettings.json placeholders. This surfaces as HTTP 400 with a
        // useful error in the toast rather than a generic 500.
        if (string.IsNullOrWhiteSpace(host)
            || string.IsNullOrWhiteSpace(portStr)
            || string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(fromAddress)
            || username!.Contains("your-email", StringComparison.OrdinalIgnoreCase)
            || password!.Contains("your-app-password", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Email isn't configured — update the Smtp section in appsettings.json with a real SMTP host, account, and app password.");
        }

        if (!int.TryParse(portStr, out var port))
            throw new InvalidOperationException($"SMTP port is not a valid number: '{portStr}'.");

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(fromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlBody };
        if (attachment is not null && !string.IsNullOrEmpty(attachmentName))
            builder.Attachments.Add(attachmentName, attachment);

        message.Body = builder.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (AuthenticationException ex)
        {
            throw new InvalidOperationException(
                "Email failed to send — SMTP credentials were rejected. Check the app password.", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Email failed to send: {ex.Message}", ex);
        }
    }
}
