using CinemaRolfoBot;
using Telegram.Bot;

string? token = Environment.GetEnvironmentVariable(Const.ENV_VAR_KEY_TOKEN);
if (string.IsNullOrEmpty(token))
{
    Console.WriteLine($"La variabile d'ambiente {Const.ENV_VAR_KEY_TOKEN} non risulta impostata.");
    return -1;
}

var telegramBotClient = new TelegramBotClient(token);

var me = await telegramBotClient.GetMeAsync();
Console.WriteLine($"Hello, World! Sono l'utente {me.Id} e il mio nome è {me.FirstName}.");

return 0;