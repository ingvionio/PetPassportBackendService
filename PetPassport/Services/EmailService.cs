using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PetPassport.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendFeedbackAsync(string type, Dictionary<string, string> fields)
    {
        var host = _config["Email:SmtpHost"] ?? throw new InvalidOperationException("Email:SmtpHost not configured");
        var port = int.Parse(_config["Email:SmtpPort"] ?? "587");
        var username = _config["Email:Username"] ?? throw new InvalidOperationException("Email:Username not configured");
        var password = _config["Email:Password"] ?? throw new InvalidOperationException("Email:Password not configured");
        var supportEmail = _config["Email:SupportEmail"] ?? username;

        var subject = type == "error"
            ? "PetPassport: Сообщение об ошибке"
            : "PetPassport: Предложение функции";

        var body = new StringBuilder();
        body.AppendLine($"<h2>{subject}</h2><hr/>");
        foreach (var (label, value) in fields)
            body.AppendLine($"<p><strong>{label}</strong><br/>{value}</p>");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("PetPassport Feedback", username));
        message.To.Add(new MailboxAddress("", supportEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body.ToString() };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
