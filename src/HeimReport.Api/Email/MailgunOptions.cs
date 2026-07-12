namespace HeimReport.Api.Email;

public class MailgunOptions
{
    public const string SectionName = "Mailgun";
    public required string ApiKey { get; init; }
    public required string Domain { get; init; }
    public required string FromEmail { get; init; }
    public required string FromName { get; init; }
    public required string VerificationBaseUrl { get; init; }
}