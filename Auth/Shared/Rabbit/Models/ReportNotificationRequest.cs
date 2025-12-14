namespace Shared.Rabbit.Models;

public class ReportNotificationRequest
{
    public string Link { get; set; }
    public long TelegramChatId { get; set; }
}