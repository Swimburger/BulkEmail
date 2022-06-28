using BulkEmail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SendGrid.Extensions.DependencyInjection;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(builder => builder.AddUserSecrets<Program>())
    .ConfigureServices((context, services) =>
    {
        services.AddOptions<SenderOptions>()
            .Bind(context.Configuration.GetSection("sender"));
        services.AddSendGrid(options => options.ApiKey = context.Configuration["SendGridApiKey"]);
        services.AddScoped<SubscriberRepository>();
        services.AddScoped<EmailSender>();
    })
    .Build();
    
var emailSender = host.Services.GetRequiredService<EmailSender>();

await emailSender.SendToSubscribers()
    .ConfigureAwait(false);