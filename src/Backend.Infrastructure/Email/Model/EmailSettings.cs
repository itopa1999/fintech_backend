namespace Infrastructure.Email.Models;

public sealed class EmailSettings
{
    public required string FromEmail { get; set; }

    public required string FromName { get; set; }

    public required string Host { get; set; }

    public int Port { get; set; }

    public required string Username { get; set; }

    public required string Password { get; set; }

    public bool UseSSL { get; set; }
}