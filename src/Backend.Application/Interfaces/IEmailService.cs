namespace Backend.Application.Interfaces;

public interface IEmailService
{
    Task SendVerificationOtpAsync(
        string firstName,
        string toEmail,
        string otp,
        CancellationToken cancellationToken = default);

    Task SendForgetPasswordEmailAsync(
        string firstName,
        string toEmail,
        string token,
        CancellationToken cancellationToken = default);
}