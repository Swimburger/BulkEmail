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
        var subscribers = subscriberRepository.GetAll();
        foreach (var subscriber in subscribers)
        {
            var message = new SendGridMessage
            {
                From = new EmailAddress(sender.Email, sender.Name),
                Subject = $"Ahoy {subscriber.FirstName}!",
                HtmlContent = $"Welcome aboard <b>{htmlEncoder.Encode(subscriber.FullName)}</b> ⚓️"
            };

            message.AddTo(new EmailAddress(subscriber.Email, subscriber.FullName));

            var response = await sendGridClient.SendEmailAsync(message);
            if (response.IsSuccessStatusCode) logger.LogInformation("Email queued");
            else logger.LogError("Email not queued");
        }
    }
}