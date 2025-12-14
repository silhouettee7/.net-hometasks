using System.Collections.Concurrent;
using System.Net.Http.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotClient.Abstractions.Bot;
using TelegramBotClient.Models;

namespace TelegramBotClient.Services;

public class TelegramBotService(
    ITelegramBotClient botClient,
    HttpClient httpClient,
    IConfiguration config,
    ILogger<TelegramBotService> logger): ITelegramBotService
{
    private readonly ConcurrentDictionary<long, UserSession> _sessions = new();
    private readonly string? _backendBaseUrl = config["Backend:BaseUrl"];

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_backendBaseUrl))
        {
            logger.LogError("BotToken or BackendBaseUrl is not configured.");
            return Task.CompletedTask;
        }
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates =
            [
                UpdateType.Message,
                UpdateType.CallbackQuery
            ]
        };
        
        botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: cancellationToken
        );
        return Task.CompletedTask;  
    }

    public async Task SendMessageAsync(long chatId, string message, CancellationToken cancellationToken = default)
    {
        await botClient.SendMessage(chatId, message, cancellationToken: cancellationToken);
    }
    
    public async Task SendReportLinkAsync(long chatId, string link, 
        CancellationToken cancellationToken = default)
    {
        await botClient.SendMessage(
            chatId: chatId,
            text: "Ваш отчет по ссылке:",
            cancellationToken: cancellationToken);
        await botClient.SendMessage(
            chatId:chatId,
            text : link,
            cancellationToken: cancellationToken);
    }

    public async Task<User> GeMeAsync(CancellationToken cancellationToken = default)
    {
        return await botClient.GetMe(cancellationToken);
    }

    async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        try
        {
            if (update.Type == UpdateType.Message && update.Message is { } message)
            {
                await HandleMessageAsync(bot, message, ct);
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery is { } callback)
            {
                await HandleCallbackAsync(bot, callback, ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Update handling error");
        }
    }

    async Task HandleMessageAsync(ITelegramBotClient bot, Message message, CancellationToken ct)
    {
        var chatId = message.Chat.Id;
        var userId = message.From!.Id;
        var text = message.Text ?? string.Empty;

        var session = _sessions.GetOrAdd(userId, _ => new UserSession());
        
        if (text == "/start")
        {
            await SendMainMenu(bot, chatId, session, ct);
            return;
        }

        switch (session.State)
        {
            case LoginState.WaitingEmail:
                session.Email = text.Trim();
                session.State = LoginState.WaitingPassword;
                await bot.SendMessage(chatId, "Введите пароль:", cancellationToken: ct);
                break;

            case LoginState.WaitingPassword:
                var password = text;
                await TryLoginAsync(bot, chatId, session, password, ct);
                break;

            default:
                await bot.SendMessage(
                    chatId,
                    "Используйте /start, чтобы открыть меню.",
                    cancellationToken: ct);
                break;
        }
    }

    async Task HandleCallbackAsync(ITelegramBotClient bot, CallbackQuery callback, CancellationToken ct)
    {
        var chatId = callback.Message!.Chat.Id;
        var userId = callback.From.Id;
        var data = callback.Data;

        var session = _sessions.GetOrAdd(userId, _ => new UserSession());

        switch (data)
        {
            case "login":
                session.State = LoginState.WaitingEmail;
                session.Email = null;
                session.Token = null;
                await bot.AnswerCallbackQuery(callback.Id, "Начинаем вход", cancellationToken: ct);
                await bot.SendMessage(chatId, "Введите email:", cancellationToken: ct);
                break;

            case "create_report":
                if (session.State != LoginState.LoggedIn || string.IsNullOrEmpty(session.Token))
                {
                    await bot.AnswerCallbackQuery(callback.Id, "Сначала войдите", cancellationToken: ct);
                    await bot.SendMessage(chatId, "Вы не авторизованы. Нажмите «Войти» в меню.", cancellationToken: ct);
                }
                else
                {
                    await bot.AnswerCallbackQuery(callback.Id, "Создаём отчёт", cancellationToken: ct);
                    await CreateReportAsync(bot, chatId, userId, session, ct);
                }
                break;
        }
    }

    async Task SendMainMenu(ITelegramBotClient bot, long chatId, UserSession session, CancellationToken ct)
    {
        List<InlineKeyboardButton> menu = new();
        if (session.State == LoginState.LoggedIn)
        {
            menu.Add(InlineKeyboardButton.WithCallbackData(
                "📊 Создать отчёт", "create_report"));
        }
        else
        {
            menu.Add(InlineKeyboardButton.WithCallbackData(
                "🔐 Войти", "login"));
        }
        var keyboard = new InlineKeyboardMarkup(menu);

        await bot.SendMessage(
            chatId,
            text: "Меню:",
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    async Task TryLoginAsync(ITelegramBotClient bot, long chatId, UserSession session, string password, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(session.Email))
        {
            await bot.SendMessage(chatId, "Сначала введите email. Нажмите /start.", cancellationToken: ct);
            session.State = LoginState.None;
            return;
        }

        var loginUrl = $"{_backendBaseUrl}/auth/telegram/login";

        var payload = new
        {
            Email = session.Email,
            Password = password,
            TelegramChatId = chatId
        };

        var response = await httpClient.PostAsJsonAsync(loginUrl, payload, ct);
        if (!response.IsSuccessStatusCode)
        {
            await bot.SendMessage(chatId, "Неверный логин или пароль.", cancellationToken: ct);
            session.State = LoginState.None;
            session.Email = null;
            session.Token = null;
            return;
        }

        var json = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
        if (json == null || string.IsNullOrEmpty(json.Token))
        {
            await bot.SendMessage(chatId, "Сервер не вернул токен.", cancellationToken: ct);
            session.State = LoginState.None;
            session.Email = null;
            session.Token = null;
            return;
        }

        session.Token = json.Token;
        session.State = LoginState.LoggedIn;

        await bot.SendMessage(chatId, "Вы успешно вошли.", cancellationToken: ct);
        await SendMainMenu(bot, chatId, session, ct);
    }

    async Task CreateReportAsync(ITelegramBotClient bot, long chatId, long userId, UserSession session, CancellationToken ct)
    {
        var url = $"{_backendBaseUrl}/users/telegram/report/create";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Headers.Add("X-Session-Token",session.Token);
        
        var response = await httpClient.SendAsync(request, ct);
        if (response.IsSuccessStatusCode)
        {
            await bot.SendMessage(
                chatId,
                "Отчёт поставлен в очередь. Когда будет готов — вы получите уведомление.",
                cancellationToken: ct);
        }
        else
        {
            await bot.SendMessage(chatId, "Не удалось создать отчёт.", cancellationToken: ct);
        }
    }

    Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiEx => $"Telegram API Error:\n[{apiEx.ErrorCode}] {apiEx.Message}",
            _ => exception.ToString()
        };
        logger.LogError(errorMessage);
        return Task.CompletedTask;
    }
}