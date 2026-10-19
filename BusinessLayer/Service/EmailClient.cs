using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using BusinessLayer.Interface;
using Model.EmailModel;

namespace BusinessLayer.Service;

public class EmailClient
{
    private readonly IEmailService emailService;
    private readonly ILogger<EmailClient> logger;

    public EmailClient(
        IEmailService emailService,
        ILogger<EmailClient> logger)
    {
        this.emailService = emailService;
        this.logger = logger;
    }

    public async Task<bool> SendEmail(
        EmailModel model)
    {
        try
        {
            await emailService.SendEmail(model);

            logger.LogInformation("Email sending completed.");

            return true;
        }
        catch (SmtpCommandException ex)
        {
            logger.LogWarning(ex, "Email delivery was rejected.");
            return false;
        }
        catch (SmtpProtocolException ex)
        {
            logger.LogError(ex, "SMTP protocol failure.");
            return false;
        }
        catch (SocketException ex)
        {
            logger.LogError(ex, "SMTP connection failed.");
            return false;
        }
    }
}