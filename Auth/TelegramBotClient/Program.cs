using Shared.Configuration.Abstractions;
using Shared.Rabbit.Abstractions;
using Shared.Rabbit.Utils;
using Telegram.Bot;
using TelegramBotClient.Abstractions.Bot;
using TelegramBotClient.Rabbit.Consumers;
using TelegramBotClient.Services;
using TelegramBotClient.Workers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<ReportNotificationQueueConsumer>();
builder.Services.AddSingleton<RabbitMqChannelAccessor>();
builder.Services.AddSingleton<IRabbitMqChannelAccessor, RabbitMqChannelAccessor>(sp => sp.GetRequiredService<RabbitMqChannelAccessor>());
builder.Services.AddSingleton<IInitializable, RabbitMqChannelAccessor>(sp => sp.GetRequiredService<RabbitMqChannelAccessor>());
builder.Services.AddHostedService<InitializerWorker>();
builder.Services.AddHostedService<TelegramBotServiceWorker>();
builder.Services.AddHostedService<ReportNotificationQueueConsumerWorker>();
builder.Services.AddSingleton<ITelegramBotClient, Telegram.Bot.TelegramBotClient>(_ =>
{
    var token = builder.Configuration["BotConfiguration:BotToken"];
    if (string.IsNullOrEmpty(token))
    {
        throw new NullReferenceException("Please provide a valid token");
    }
    return new Telegram.Bot.TelegramBotClient(token);
});
builder.Services.AddSingleton<ITelegramBotService, TelegramBotService>();
builder.Services.AddSingleton<HttpClient>();
var host = builder.Build();
host.Run();