using CinemaRolfoBot;
using CinemaRolfoBot.Model.Json;
using System.Text.Json;
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

// Call asynchronous network methods in a try/catch block to handle exceptions.
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