using SmileBotFirstConnection;
using SmileBotFirstConnection.MetApi;
using Telegram.Bot;

var metApi = new MetApi();
var botClient = new TelegramBotClient("TOKEN");

var metBot = new BotEngine(botClient, metApi);

await metBot.ListenForMessagesAsync();