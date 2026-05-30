namespace Backend.Application.Interfaces;

public interface IEmailService
{
    Task SendVerificationOtpAsync(
        string toEmail,
        string otp,
        CancellationToken cancellationToken = default);
}