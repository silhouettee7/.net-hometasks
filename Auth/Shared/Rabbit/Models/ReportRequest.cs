namespace Shared.Rabbit.Models;

public class ReportRequest
{
    public Guid ReportId { get; set; }
    public long TelegramChatId { get; set; }
}