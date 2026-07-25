using System.Net;
using HeimReport.Api.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;

namespace HeimReport.Api.Email;

public sealed partial class MailgunEmailSender(
    IOptions<MailgunOptions> options,
    ILogger<MailgunEmailSender> logger)
    : IEmailSender
{
    private readonly MailgunOptions _options = options.Value;

    public async Task SendEmailVerificationAsync(
        string toEmail,
        string token,
        Language language,
        CancellationToken cancellationToken = default)
    {
        var verificationLink = $"{_options.VerificationBaseUrl}?token={Uri.EscapeDataString(token)}";
        var (subject, htmlBody) = BuildVerificationTemplate(language, verificationLink);

        await SendAsync(toEmail, subject, htmlBody, cancellationToken);

        LogVerificationEmailSent(toEmail, language);
    }

    public async Task SendTemporaryPasswordAsync(
        string toEmail,
        string temporaryPassword,
        Language language,
        CancellationToken cancellationToken = default)
    {
        var (subject, htmlBody) = BuildTemporaryPasswordTemplate(language, temporaryPassword);

        await SendAsync(toEmail, subject, htmlBody, cancellationToken);

        LogTemporaryPasswordEmailSent(toEmail, language);
    }

    private async Task SendAsync(
        string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
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

        request.AddParameter("from", $"{_options.FromName} <{_options.FromEmail}>");
        request.AddParameter("to", toEmail);
        request.AddParameter("subject", subject);
        request.AddParameter("html", htmlBody);

        var response = await client.ExecuteAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK || !response.IsSuccessful)
        {
            LogEmailSendFailed(toEmail, response.StatusCode, response.ErrorMessage ?? response.Content);

            throw new InvalidOperationException(
                $"Failed to send email via Mailgun. Status: {response.StatusCode}");
        }
    }

    private static (string Subject, string Html) BuildVerificationTemplate(
        Language language, string verificationLink)
    {
        return language switch
        {
            Language.Spanish => (
                "Verifica tu cuenta de HeimReport",
                BuildVerificationHtmlEs(verificationLink)),

            _ => (
                "Verify your HeimReport account",
                BuildVerificationHtmlEn(verificationLink))
        };
    }

    private static (string Subject, string Html) BuildTemporaryPasswordTemplate(
        Language language, string temporaryPassword)
    {
        return language switch
        {
            Language.Spanish => (
                "Tu contraseña temporal de HeimReport",
                BuildTemporaryPasswordHtmlEs(temporaryPassword)),

            _ => (
                "Your temporary HeimReport password",
                BuildTemporaryPasswordHtmlEn(temporaryPassword))
        };
    }

    private static string BuildVerificationHtmlEn(string verificationLink) => $"""
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

    private static string BuildVerificationHtmlEs(string verificationLink) => $"""
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

    private static string BuildTemporaryPasswordHtmlEn(string temporaryPassword) => $"""
        <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
            <h2>Your HeimReport account is ready</h2>
            <p>An administrator has created an account for you. Use the temporary password below to log in:</p>
            <p style="text-align: center; margin: 32px 0;">
                <span style="background-color: #F3F4F6; color: #111827; padding: 12px 24px;
                             border-radius: 6px; display: inline-block; font-family: monospace; font-size: 18px;">
                    {temporaryPassword}
                </span>
            </p>
            <p>For your security, we recommend changing this password as soon as you log in.</p>
            <p style="color: #6B7280; font-size: 12px; margin-top: 32px;">
                If you weren't expecting this email, please contact your HR department.
            </p>
        </div>
        """;

    private static string BuildTemporaryPasswordHtmlEs(string temporaryPassword) => $"""
        <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
            <h2>Tu cuenta de HeimReport está lista</h2>
            <p>Un administrador ha creado una cuenta para ti. Usa la siguiente contraseña temporal para iniciar sesión:</p>
            <p style="text-align: center; margin: 32px 0;">
                <span style="background-color: #F3F4F6; color: #111827; padding: 12px 24px;
                             border-radius: 6px; display: inline-block; font-family: monospace; font-size: 18px;">
                    {temporaryPassword}
                </span>
            </p>
            <p>Por tu seguridad, te recomendamos cambiar esta contraseña tan pronto inicies sesión.</p>
            <p style="color: #6B7280; font-size: 12px; margin-top: 32px;">
                Si no esperabas este correo, por favor contacta a tu departamento de Recursos Humanos.
            </p>
        </div>
        """;

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to send email to {Email}. Status: {StatusCode}. Error: {Error}")]
    private partial void LogEmailSendFailed(string email, HttpStatusCode statusCode, string? error);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Verification email sent to {Email} in {Language}")]
    private partial void LogVerificationEmailSent(string email, Language language);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Temporary password email sent to {Email} in {Language}")]
    private partial void LogTemporaryPasswordEmailSent(string email, Language language);
}