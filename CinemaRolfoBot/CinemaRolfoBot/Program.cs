using CinemaRolfoBot;
using CinemaRolfoBot.Model.DB;
using CinemaRolfoBot.Model.Json;
using System.Text.Json;
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

try
{
    HttpClient client = new HttpClient();
    string responseBody = await client.GetStringAsync(Const.TSB_FILMS_WITH_SHOWING_URL);

    FilmsWithShowings? filmsWithShowings = JsonSerializer.Deserialize<FilmsWithShowings>(responseBody);

    Console.WriteLine(filmsWithShowings);
}
catch (HttpRequestException e)
{
    Console.WriteLine("\nException Caught!");
    Console.WriteLine("Message :{0} ", e.Message);
}

return 0;

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