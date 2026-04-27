namespace VehiclePartsSystem.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string htmlBody, byte[]? attachment = null, string? attachmentName = null);
}
