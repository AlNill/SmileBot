using SmileBotFirstConnection;
using Telegram.Bot;

var botClient = new TelegramBotClient("");

var metBot = new BotEngine(botClient);
await metBot.ListenForMessagesAsync();