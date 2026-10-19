namespace BusinessLayer.Interface;
using Model.EmailModel;
public interface IEmailService
{
    Task SendEmail(EmailModel email);
}