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
    private readonly SenderOptions sender;

    public EmailSender(
        SubscriberRepository subscriberRepository, 
        ISendGridClient sendGridClient,
        IOptions<SenderOptions> senderOptions,
        ILogger<EmailSender> logger
    )
    {
        this.subscriberRepository = subscriberRepository;
        this.sendGridClient = sendGridClient;
        this.logger = logger;
        this.sender = senderOptions.Value;
    }

    public async Task SendToSubscribers()
    {
        var subscribers = subscriberRepository.GetAllSubscribers();

        foreach (var subscriber in subscribers)
        {
            var message = new SendGridMessage()
            {
                From = new EmailAddress(sender.Email, sender.Name),
                Subject = "Ahoy matey!",
                HtmlContent = @"Welcome aboard friend ⚓️"
            };
            message.AddTo(subscriber.Email, subscriber.FullName);
        
            var response = await sendGridClient.SendEmailAsync(message);
            if(response.IsSuccessStatusCode) logger.LogInformation("Email queued");
            else logger.LogError("Email not queued");
        }
    }
}