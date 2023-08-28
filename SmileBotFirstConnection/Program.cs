using Telegram.Bot;

var botClient = new TelegramBotClient("Your_token");

var me = await botClient.GetMeAsync();
Console.WriteLine($"Hello, World! I am bot {me.Id} and my name is {me.FirstName}.");