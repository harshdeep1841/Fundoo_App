using BusinessLayer.Interface;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Model.EmailModel;

namespace EmailController.Controller;
[ApiController]
[Route("[controller]")]
public class EmailController : ControllerBase
{
    IEmailService emailService;

    public EmailController(IEmailService emailService)
    {
        this.emailService = emailService;
    }
    
    [HttpGet]
    public IActionResult defaultRoute()
    {
        return Ok("Email project is running");
    }
    
    [HttpPost("send")]
    public async Task<IActionResult> SendEmail(EmailModel model)
    {
        await emailService.SendEmail(model);

        return Ok("Email sent successfully");
    }
}