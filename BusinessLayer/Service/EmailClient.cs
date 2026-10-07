using System.Net.Http.Json;

namespace BusinessLayer.Service;

public class EmailClient
{
    public HttpClient Client { get; }
    public EmailClient(HttpClient client)
    {
        Client = client;
    }

    public async Task SendEmail(EmailModel.Model.EmailModel model)
    {
        await Client.PostAsJsonAsync("http://localhost:5015/email/send", model);
    }
}