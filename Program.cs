using System.Text.Encodings.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SendGrid.Extensions.DependencyInjection;
using BulkEmail;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(builder => 
        builder
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<Program>()
            .AddCommandLine(args)
    )
    .ConfigureServices((context, services) =>
    {
        services.AddOptions<SenderOptions>()
            .Bind(context.Configuration.GetSection("sender"));
        services.AddSendGrid(options => options.ApiKey = context.Configuration["SendGridApiKey"]);
        services.AddScoped<SubscriberRepository>();
        services.AddScoped<EmailSender>();
        services.AddTransient<HtmlEncoder>(_ => HtmlEncoder.Default);
    })
    .Build();

var emailSender = host.Services.GetRequiredService<EmailSender>();

await emailSender.SendToSubscribers()
    .ConfigureAwait(false);