using CinemaRolfoBot;
using CinemaRolfoBot.Model.Beans;
using CinemaRolfoBot.Utils;
using log4net;
using log4net.Config;
using System.Reflection;
using Telegram.Bot;

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
ILog Log = LogManager.GetLogger(typeof(Program));

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

if (!int.TryParse(Environment.GetEnvironmentVariable(Const.ENV_VAR_KEY_UPDATE_FREQ_SECONDS), out int updateFrequencySeconds))
{
    Log.Fatal($"The environment variable '{Const.ENV_VAR_KEY_UPDATE_FREQ_SECONDS}' isn't correctly setted.");
    return -1;
}

if (!int.TryParse(Environment.GetEnvironmentVariable(Const.ENV_VAR_KEY_RESET_FREQ_MINUTES), out int updateResetMinutes))
{
    Log.Fatal($"The environment variable '{Const.ENV_VAR_KEY_RESET_FREQ_MINUTES}' isn't correctly setted.");
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

BotManager botManager = new BotManager(Token);

CancellationToken CancellationToken = new CancellationToken();
Task task = PeriodicUpdateDB(dbManager, TimeSpan.FromSeconds(updateFrequencySeconds), TimeSpan.FromMinutes(updateResetMinutes), CancellationToken);

while (true)
    await Task.Delay(TimeSpan.FromMinutes(1), CancellationToken);

return -1;

async Task PeriodicUpdateDB(DbManager dbManager, TimeSpan updateFreqSeconds, TimeSpan resetFreqMinutes, CancellationToken cancellationToken)
{
    while (true)
    {
        string filmsWithShowings = await GetTheSpaceFilmsWithShowings();
        dbManager.UpdateDB(filmsWithShowings, out UpdateDBOutput output);
        DateTime? lastReset = dbManager.GetRunningInfo(CinemaRolfoBot.Model.DB.ERunningInfoId.LastReset);
        if (lastReset == null || lastReset.Value.Add(resetFreqMinutes) <= DateTime.Now)
        {
            dbManager.WipeDB();
            dbManager.UpdateDB(filmsWithShowings, out UpdateDBOutput _);
        }
        await Task.Delay(updateFreqSeconds, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            break;
    }
}

async Task<string> GetTheSpaceFilmsWithShowings()
{
    string responseBody = "";
    try
    {
        responseBody = await Const.HttpClient.GetStringAsync(Const.TSB_FILMS_WITH_SHOWING_URL);
    }
    catch (HttpRequestException e)
    {
        //TODO Notify errors to mantainers users
        Log.Error($"HTTP error on getting '{Const.TSB_FILMS_WITH_SHOWING_URL}'. Error message: {e.Message}");
    }
    return responseBody;
}