using HeimReport.Api.Enums;

namespace HeimReport.Api.Email;

public interface IEmailSender
{
    Task SendEmailVerificationAsync(
        string toEmail,
        string token,
        Language language,
        CancellationToken cancellationToken = default);
}
