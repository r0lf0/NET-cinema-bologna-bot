using CinemaRolfoBot;
using CinemaRolfoBot.Model.Beans;
using CinemaRolfoBot.Model.DB;
using CinemaRolfoBot.Model.Json;
using log4net;
using log4net.Config;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Telegram.Bot;

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
ILog Log = LogManager.GetLogger(typeof(Program));

var pippo = new FileInfo($"log4net.config");
_ = XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()), new FileInfo($"log4net.config"));
Log.Info("Logger started");

string? Token = Environment.GetEnvironmentVariable(Const.ENV_VAR_KEY_TOKEN);
if (string.IsNullOrEmpty(Token))
{
    Log.Fatal($"The environment variable '{Const.ENV_VAR_KEY_TOKEN}' isn't correctly setted.");
    return -1;
}

string? PostgresConnectionString = Environment.GetEnvironmentVariable(Const.ENV_VAR_KEY_POSTGRESSQL);
if (string.IsNullOrEmpty(PostgresConnectionString))
{
    Log.Fatal($"The environment variable '{Const.ENV_VAR_KEY_POSTGRESSQL}' isn't correctly setted.");
    return -1;
}

if (!int.TryParse(Environment.GetEnvironmentVariable(Const.ENV_VAR_KEY_UPDATE_FREQ), out int updateFrequencySeconds))
{
    Log.Fatal($"The environment variable '{Const.ENV_VAR_KEY_UPDATE_FREQ}' isn't correctly setted.");
    return -1;
}

DbManager dbManager;
try
{
    dbManager = new DbManager(PostgresConnectionString);
}
catch (Exception ex)
{
    Log.Fatal($"Can't connect to database with connection string '{PostgresConnectionString}'. Exception message: {ex.Message}");
    return -1;
}

TelegramBotClient telegramBotClient = await InitBot(Token);

CancellationToken CancellationToken = new CancellationToken();
Task task = PeriodicUpdateDB(dbManager, TimeSpan.FromSeconds(updateFrequencySeconds), CancellationToken);

while (true)
    await Task.Delay(TimeSpan.FromMinutes(1), CancellationToken);

return -1;

async Task<TelegramBotClient> InitBot(string Token)
{
    TelegramBotClient? telegramBotClient = new TelegramBotClient(Token);

    var me = await telegramBotClient.GetMeAsync();
    Log.Info($"Telegram bot '{me.FirstName}' with id '{me.Id}' correctly initialized.");

    return telegramBotClient;
}

async Task PeriodicUpdateDB(DbManager dbManager, TimeSpan interval, CancellationToken cancellationToken)
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