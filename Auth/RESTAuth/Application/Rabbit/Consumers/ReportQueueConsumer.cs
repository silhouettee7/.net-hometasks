using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RESTAuth.Application.Rabbit.Producers;
using RESTAuth.Domain.Abstractions.Services;
using Shared.Rabbit.Abstractions;
using Shared.Rabbit.Models;

namespace RESTAuth.Application.Rabbit.Consumers;

public class ReportQueueConsumer(
    ILogger<ReportQueueConsumer> logger,
    IRabbitMqChannelAccessor accessor, 
    ReportNotificationQueueProducer reportNotificationQueueProducer,
    IUserService userService,
    IConfiguration configuration): RabbitConfigurer(accessor, configuration, "Reports")
{

    public async Task ConsumeAsync()
    {
        await ConfigureRabbitAsync(ExchangeType.Direct, true);
        
        await _channel!.BasicQosAsync(0,  3,false);
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += DequeueAsync;
        await _channel.BasicConsumeAsync(_queueName, false, consumer);
    }

    private async Task DequeueAsync(object _, BasicDeliverEventArgs args)
    {
        try
        {
            var json = Encoding.UTF8.GetString(args.Body.ToArray());
            var request = JsonSerializer.Deserialize<ReportRequest>(json);
            var linkResult = await userService.GenerateReportOnUsersAndReturnLink(request.ReportId);
            if (!linkResult.IsSuccess)
            {
                logger.LogError(linkResult!.AppError.Message);
                await _channel!.BasicNackAsync(args.DeliveryTag, false, true);
                return;
            }

            var link = linkResult.Value;
            var body = new ReportNotificationRequest
            {
                Link = link,
                TelegramChatId = request.TelegramChatId,
            };
            await _channel!.BasicAckAsync(args.DeliveryTag, false);
            await reportNotificationQueueProducer.EnqueueNotificationAsync(body);
        }
        catch (Exception ex)
        {
            logger.LogError("Ошибка при обработке отчёта: \n" + ex);
            await _channel!.BasicNackAsync(args.DeliveryTag, false, true);
        }
    }
}