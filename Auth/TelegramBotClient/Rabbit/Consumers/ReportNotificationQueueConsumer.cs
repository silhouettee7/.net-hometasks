using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Rabbit.Abstractions;
using Shared.Rabbit.Models;
using TelegramBotClient.Abstractions.Bot;
using ExchangeType = RabbitMQ.Client.ExchangeType;

namespace TelegramBotClient.Rabbit.Consumers;

public class ReportNotificationQueueConsumer(
    ITelegramBotService telegramBotService,
    IRabbitMqChannelAccessor accessor,
    IConfiguration  configuration, 
    ILogger<ReportNotificationQueueConsumer> logger)
    : RabbitConfigurer(accessor, configuration, "Notifications")
{

    public async Task ConsumeAsync()
    {
        await ConfigureRabbitAsync(ExchangeType.Direct, true);
        await _channel!.BasicQosAsync(0, 3, false);
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += DequeueAsync;
        await _channel.BasicConsumeAsync(_queueName, false, consumer);
    }
    
    private async Task DequeueAsync(object _, BasicDeliverEventArgs args)
    {
        try
        {
            var json = Encoding.UTF8.GetString(args.Body.ToArray());
            var body = JsonSerializer.Deserialize<ReportNotificationRequest>(json);
            await telegramBotService.SendReportLinkAsync(body.TelegramChatId, body.Link);
            await _channel!.BasicAckAsync(args.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            await _channel!.BasicNackAsync(args.DeliveryTag, false, true);
            logger.LogError(ex, "Ошибка при отправки уведомления");
        }
    }
}