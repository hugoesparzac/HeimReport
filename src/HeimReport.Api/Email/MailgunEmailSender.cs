using System.Net;
using HeimReport.Api.Enums;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;

namespace HeimReport.Api.Email;

public sealed class MailgunEmailSender(IOptions<MailgunOptions> options, ILogger<MailgunEmailSender> logger)
    : IEmailSender
{
    private readonly MailgunOptions _options = options.Value;

    public async Task SendEmailVerificationAsync(
        string toEmail,
        string token,
        Language language,
        CancellationToken cancellationToken = default)
    {
        var restClientOptions = new RestClientOptions("https://api.mailgun.net")
        {
            Authenticator = new HttpBasicAuthenticator("api", _options.ApiKey)
        };

        using var client = new RestClient(restClientOptions);

        var request = new RestRequest($"/v3/{_options.Domain}/messages", Method.Post)
        {
            AlwaysMultipartFormData = true
        };

        var verificationLink = $"{_options.VerificationBaseUrl}?token={Uri.EscapeDataString(token)}";
        var (subject, htmlBody) = BuildTemplate(language, verificationLink);

        request.AddParameter("from", $"{_options.FromName} <{_options.FromEmail}>");
        request.AddParameter("to", toEmail);
        request.AddParameter("subject", subject);
        request.AddParameter("html", htmlBody);

        var response = await client.ExecuteAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK || !response.IsSuccessful)
        {
            logger.LogError(
                "Failed to send verification email to {Email}. Status: {StatusCode}. Error: {Error}",
                toEmail, response.StatusCode, response.ErrorMessage ?? response.Content);

            throw new InvalidOperationException(
                $"Failed to send verification email via Mailgun. Status: {response.StatusCode}");
        }

        logger.LogInformation("Verification email sent to {Email} in {Language}", toEmail, language);
    }

    private static (string Subject, string Html) BuildTemplate(Language language, string verificationLink)
    {
        return language switch
        {
            Language.Spanish => ("Verifica tu cuenta de HeimReport", BuildSpanishHtml(verificationLink)),
            _ => ("Verify your HeimReport account", BuildEnglishHtml(verificationLink))
        };
    }

    private static string BuildEnglishHtml(string verificationLink) => $"""
        <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
            <h2>Verify your email address</h2>
            <p>Thanks for registering with HeimReport. Please confirm your email address by clicking the button below:</p>
            <p style="text-align: center; margin: 32px 0;">
                <a href="{verificationLink}"
                   style="background-color: #4F46E5; color: #ffffff; padding: 12px 24px;
                          text-decoration: none; border-radius: 6px; display: inline-block;">
                    Verify Email
                </a>
            </p>
            <p>If the button doesn't work, copy and paste this link into your browser:</p>
            <p style="word-break: break-all; color: #4F46E5;">{verificationLink}</p>
            <p style="color: #6B7280; font-size: 12px; margin-top: 32px;">
                If you didn't create an account with HeimReport, you can safely ignore this email.
            </p>
        </div>
        """;

    private static string BuildSpanishHtml(string verificationLink) => $"""
        <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
            <h2>Verifica tu correo electrónico</h2>
            <p>Gracias por registrarte en HeimReport. Por favor confirma tu correo electrónico haciendo clic en el siguiente botón:</p>
            <p style="text-align: center; margin: 32px 0;">
                <a href="{verificationLink}"
                   style="background-color: #4F46E5; color: #ffffff; padding: 12px 24px;
                          text-decoration: none; border-radius: 6px; display: inline-block;">
                    Verificar correo
                </a>
            </p>
            <p>Si el botón no funciona, copia y pega este enlace en tu navegador:</p>
            <p style="word-break: break-all; color: #4F46E5;">{verificationLink}</p>
            <p style="color: #6B7280; font-size: 12px; margin-top: 32px;">
                Si no creaste una cuenta en HeimReport, puedes ignorar este correo con seguridad.
            </p>
        </div>
        """;
}
