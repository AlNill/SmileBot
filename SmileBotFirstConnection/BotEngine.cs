using SmileBotFirstConnection.MetApi;
using SmileBotFirstConnection.Models;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SmileBotFirstConnection;

public class BotEngine
{
    private readonly TelegramBotClient _botClient;
    private static IMetApi? _metApi;
    private static IReplyMarkup _keyboard;

    public BotEngine(TelegramBotClient botClient, IMetApi metApi)
    {
        _botClient = botClient;
        _metApi = metApi;

        KeyboardButton btn = new KeyboardButton("!random");
        _keyboard = new ReplyKeyboardMarkup(btn);
    }

    public async Task ListenForMessagesAsync()
    {
        using var cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };
        _botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );       

        var me = await _botClient.GetMeAsync();

        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
        {
            return;
        }

        if (message.Text is not { } messageText)
        {
            return;
        }

        Console.WriteLine($"Received a '{messageText}' message in chat {message.Chat.Id}.");

        if(message.Text == "/start")
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "I am working. Choose a command.",
                replyMarkup: _keyboard,
                cancellationToken: cancellationToken);
        }

        if (message.Text == "!random")
        {
            var randomCollectionItem = await RandomImageRequestAsync();

            await SendPhotoMessageAsync(botClient, message, randomCollectionItem, cancellationToken);
        }

        if (message.Text.Contains("!search"))
        {
            var collectionItem = await SearchImageRequestAsync(message);

            if (!string.IsNullOrEmpty(collectionItem.primaryImage))
            {
                await SendPhotoMessageAsync(botClient, message, collectionItem, cancellationToken);
            }
        }
    }

    private static async Task SendPhotoMessageAsync(ITelegramBotClient botClient, Message message, CollectionItem collectionItem, CancellationToken cancellationToken)
    {
        Message sendArtwork = await botClient.SendPhotoAsync(
            chatId: message.Chat.Id,
            photo: collectionItem.primaryImage,
            caption: "<b>" + collectionItem.artistDisplayName + "</b>" + " <i>Artwork</i>: " + collectionItem.title,
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken,
            replyMarkup: _keyboard);
    }

    private static async Task<CollectionItem> SearchImageRequestAsync(Message message)
    {
        string[] s = message.Text.Split(" ");

        var searchList = await _metApi.SearchCollectionAsync(s[1]);

        var collectionObject = HelperMethods.RandomNumberFromList(searchList.objectIDs);

        var collectionItem = await _metApi.GetCollectionItemAsync(collectionObject.ToString());

        return collectionItem;
    }

    private static async Task<CollectionItem> RandomImageRequestAsync()
    {
        var objectList = await _metApi.GetCollectionObjectsAsync();

        var validImage = false;

        while (!validImage)
        {
            var collectionObject = HelperMethods.RandomNumberFromList(objectList.objectIDs);

            var collectionItem = await _metApi.GetCollectionItemAsync(collectionObject.ToString());

            if (!string.IsNullOrEmpty(collectionItem.primaryImage))
            {
                validImage = true;

                return collectionItem;
            }
        }

        throw new Exception("Error: Can't get random image");
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}