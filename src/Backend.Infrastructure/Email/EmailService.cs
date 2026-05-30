using Backend.Application.Interfaces;
using Infrastructure.Email.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Backend.Infrastructure.Email;

public sealed class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> options,
        ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendVerificationOtpAsync(
        string toEmail,
        string otp,
        CancellationToken cancellationToken = default)
    {
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "Email",
            "Templates",
            "VerificationOtp.html");

        var htmlTemplate = await File.ReadAllTextAsync(
            templatePath,
            cancellationToken);

        var htmlBody = htmlTemplate.Replace(
            "{{otp}}",
            otp);

        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(
            _settings.FromName,
            _settings.FromEmail));

        email.To.Add(MailboxAddress.Parse(toEmail));

        email.Subject = "Email Verification OTP";

        email.Body = new BodyBuilder
        {
            HtmlBody = htmlBody
        }.ToMessageBody();

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _settings.Host,
            _settings.Port,
            _settings.UseSSL,
            cancellationToken);

        await smtp.AuthenticateAsync(
            _settings.Username,
            _settings.Password,
            cancellationToken);

        await smtp.SendAsync(
            email,
            cancellationToken);

        await smtp.DisconnectAsync(
            true,
            cancellationToken);

        _logger.LogInformation(
            "Verification OTP email sent to {Email}",
            toEmail);
    }
}