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

    public async Task SendForgetPasswordEmailAsync(string firstName, string toEmail, string token, CancellationToken cancellationToken = default)
    {
        await SendEmailAsync(
            toEmail,
            "ForgetPassword.html",
            "Forget Password",
            new Dictionary<string, string> { { "{{firstName}}", firstName }, { "{{token}}", token } },
            cancellationToken);
    }

    public async Task SendVerificationOtpAsync(string firstName, string toEmail, string otp, CancellationToken cancellationToken = default)
    {
        await SendEmailAsync(
            toEmail,
            "VerificationOtp.html",
            "Email Verification OTP",
            new Dictionary<string, string> { { "{{firstName}}", firstName }, { "{{otp}}", otp } },
            cancellationToken);
    }

    private async Task SendEmailAsync(
        string toEmail,
        string templateFileName,
        string subject,
        Dictionary<string, string> replacements,
        CancellationToken cancellationToken)
    {
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "Email",
            "Templates",
            templateFileName);

        var htmlTemplate = await File.ReadAllTextAsync(templatePath, cancellationToken);

        var htmlBody = replacements.Aggregate(htmlTemplate, (current, replacement) => 
            current.Replace(replacement.Key, replacement.Value));

        // Build email message
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        if (_settings.UseConsoleEmail)
        {
            var logMessage = $"""
                [CONSOLE EMAIL] To: {toEmail}
                Subject: {subject}
                Body:
                {htmlBody}
                """;
            _logger.LogInformation(logMessage);
            
            Console.WriteLine(logMessage);
            await Task.CompletedTask;
            return;
        }

        // Send via SMTP
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSSL, cancellationToken);
        await smtp.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
        await smtp.SendAsync(email, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Email sent to {Email} with subject {Subject}", toEmail, subject);
    }
}