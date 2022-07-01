using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BulkEmail;

public class EmailSender
{
    private readonly SubscriberRepository subscriberRepository;
    private readonly ISendGridClient sendGridClient;
    private readonly ILogger<EmailSender> logger;
    private readonly HtmlEncoder htmlEncoder;
    private readonly SenderOptions sender;

    public EmailSender(
        SubscriberRepository subscriberRepository,
        ISendGridClient sendGridClient,
        IOptions<SenderOptions> senderOptions,
        ILogger<EmailSender> logger,
        HtmlEncoder htmlEncoder
    )
    {
        this.subscriberRepository = subscriberRepository;
        this.sendGridClient = sendGridClient;
        this.logger = logger;
        this.htmlEncoder = htmlEncoder;
        this.sender = senderOptions.Value;
    }

    public async Task SendToSubscribers()
    {
        var sendAt = DateTimeOffset.Now.AddMinutes(5).ToUnixTimeSeconds();
    
        var pageSize = 1_000;
        var subscriberCount = subscriberRepository.Count();
        var amountOfPages = (int) Math.Ceiling((double) subscriberCount / pageSize);

        for (var pageIndex = 0; pageIndex < amountOfPages; pageIndex++)
        {
            var subscribers = subscriberRepository.GetByPage(pageSize, pageIndex);
        
            var message = new SendGridMessage
            {
                From = new EmailAddress(sender.Email, sender.Name),
                Subject = "Ahoy -FirstName_Raw-!",
                HtmlContent = "Welcome aboard <b>-FullName-</b> ⚓️",

                // max 1000 Personalizations
                Personalizations = subscribers.Select(s => new Personalization
                {
                    Tos = new List<EmailAddress> {new EmailAddress(s.Email, s.FullName)},
                    // Substitutions data is max 10,000 bytes per Personalization object
                    Substitutions = new Dictionary<string, string>
                    {
                        {"-FirstName_Raw-", s.FirstName},
                        {"-FullName-", htmlEncoder.Encode(s.FullName)}
                    }
                }).ToList(),
            
                // max 72 hours from now
                SendAt = sendAt
            };

            var response = await sendGridClient.SendEmailAsync(message);
            if (response.IsSuccessStatusCode) logger.LogInformation("Email queued");
            else logger.LogError("Email not queued");
        }
    }
}