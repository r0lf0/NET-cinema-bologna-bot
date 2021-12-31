using CinemaRolfoBot;
using CinemaRolfoBot.Model.Beans;
using CinemaRolfoBot.Model.DB;
using CinemaRolfoBot.Model.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Telegram.Bot;

string? Token = Environment.GetEnvironmentVariable(Const.ENV_VAR_KEY_TOKEN);
if (string.IsNullOrEmpty(Token))
{
    Console.WriteLine($"La variabile d'ambiente {Const.ENV_VAR_KEY_TOKEN} non risulta impostata.");
    return -1;
}

string? PostgresConnectionString = Environment.GetEnvironmentVariable(Const.ENV_VAR_KEY_POSTGRESSQL);
if (string.IsNullOrEmpty(PostgresConnectionString))
{
    Console.WriteLine($"La variabile d'ambiente {Const.ENV_VAR_KEY_POSTGRESSQL} non risulta impostata.");
    return -1;
}

if (!int.TryParse(Environment.GetEnvironmentVariable(Const.ENV_VAR_KEY_UPDATE_FREQ), out int updateFrequencySeconds))
{
    Console.WriteLine($"La variabile d'ambiente {Const.ENV_VAR_KEY_UPDATE_FREQ} non risulta correttamente impostata.");
    return -1;
}

TelegramBotClient telegramBotClient = await InitBot(Token);
DBManager dbManager = new DBManager(PostgresConnectionString);

CancellationToken CancellationToken = new CancellationToken();
Task task = PeriodicUpdateDB(dbManager, TimeSpan.FromSeconds(updateFrequencySeconds), CancellationToken);

while (true)
    await Task.Delay(TimeSpan.FromMinutes(1), CancellationToken);

return -1;

async Task<TelegramBotClient> InitBot(string Token)
{
    TelegramBotClient? telegramBotClient = new TelegramBotClient(Token);

    var me = await telegramBotClient.GetMeAsync();
    Console.WriteLine($"Hello, World! Sono l'utente {me.Id} e il mio nome è {me.FirstName}.");

    return telegramBotClient;
}

async Task PeriodicUpdateDB(DBManager dbManager, TimeSpan interval, CancellationToken cancellationToken)
{
    string lastFilmsWithShowing = "";
    while (true)
    {
        lastFilmsWithShowing = dbManager.UpdateDB(lastFilmsWithShowing, out UpdateDBOutput output);

        await Task.Delay(interval, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            break;
    }
}