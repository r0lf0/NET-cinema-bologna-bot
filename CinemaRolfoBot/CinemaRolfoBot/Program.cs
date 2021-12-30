using CinemaRolfoBot;
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

TelegramBotClient telegramBotClient = await InitBot(Token);
CinemaContext context = InitDB(PostgresConnectionString);

CancellationToken CancellationToken = new CancellationToken();
Task task = PeriodicUpdateDB(TimeSpan.FromMinutes(5), CancellationToken);

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

CinemaContext InitDB(string PostgresConnectionString)
{
    CinemaContext Context = new CinemaContext(PostgresConnectionString);
    return Context;
}

async Task PeriodicUpdateDB(TimeSpan interval, CancellationToken cancellationToken)
{
    string lastFilmsWithShowing = "";
    while (true)
    {
        lastFilmsWithShowing = await UpdateDB(lastFilmsWithShowing);

        await Task.Delay(interval, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            break;
    }
}

async Task<string> UpdateDB(string lastFilmsWithShowing)
{
    string responseBody = "";
    try
    {
        HttpClient client = new();
        responseBody = await client.GetStringAsync(Const.TSB_FILMS_WITH_SHOWING_URL);
    }
    catch (HttpRequestException e)
    {
        //TODO Notify errors to mantainers users
        Console.WriteLine($"Errore nella richiesta HTTP verso {Const.TSB_FILMS_WITH_SHOWING_URL}. Error message: {e.Message}");
        return lastFilmsWithShowing;
    }

    //Comparing with previous responseBody
    string currentFilmsWithShowings = Regex.Replace(responseBody, @"\s+", String.Empty);
    if (lastFilmsWithShowing == currentFilmsWithShowings)
        return currentFilmsWithShowings;

    FilmsWithShowings? filmsWithShowings = null;
    try
    {
        filmsWithShowings = JsonSerializer.Deserialize<FilmsWithShowings>(responseBody);
    }
    catch (JsonException e)
    {
        //TODO Notify errors to mantainers users
        Console.WriteLine($"Errore nel parsing JSON della response da {Const.TSB_FILMS_WITH_SHOWING_URL}. Error message: {e.Message}");
        return lastFilmsWithShowing;
    }

    return currentFilmsWithShowings;
}