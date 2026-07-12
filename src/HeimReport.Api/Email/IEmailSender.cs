namespace HeimReport.Api.Email;

public interface IEmailSender
{
    Task SendEmailVerificationAsync(string toEmail, string token, CancellationToken cancellationToken = default);
}