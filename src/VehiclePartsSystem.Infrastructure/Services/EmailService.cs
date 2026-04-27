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

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(smtp["FromAddress"]));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlBody };
        if (attachment is not null && !string.IsNullOrEmpty(attachmentName))
            builder.Attachments.Add(attachmentName, attachment);

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(smtp["Host"], int.Parse(smtp["Port"]!), SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(smtp["Username"], smtp["Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
