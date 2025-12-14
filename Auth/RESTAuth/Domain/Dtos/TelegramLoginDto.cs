namespace RESTAuth.Domain.Dtos;

public class TelegramLoginDto: AbstractLoginDto
{
    public long TelegramChatId { get; set; }
}