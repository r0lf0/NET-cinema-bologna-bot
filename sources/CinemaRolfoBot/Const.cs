namespace CinemaRolfoBot
{
    public class Const
    {
        public static readonly HttpClient HttpClient = new HttpClient();

        /// <summary>
        /// La variabile d'ambiente in cui deve essere impostato il token ottenuto da @BotFather
        /// </summary>
        public const string ENV_VAR_KEY_TOKEN = "TOKEN";

        /// <summary>
        /// La variabile d'ambiente in cui deve essere impostata la stringa di connessione al DB PostgresSQL
        /// </summary>
        public const string ENV_VAR_KEY_POSTGRESSQL = "POSTGRESSQL";

        public const string ENV_VAR_KEY_UPDATE_FREQ_SECONDS = "UPDATE_FREQ_SECONDS";
        public const string ENV_VAR_KEY_RESET_FREQ_MINUTES = "RESET_FREQ_MINUTES";

        public const string TSB_FILMS_WITH_SHOWING_URL = @"https://www.thespacecinema.it/data/filmswithshowings/3";

        public static readonly IEnumerable<string> Commands_Help = new List<string> { "/aiuto", "/help", "/start" };
        public static readonly IEnumerable<string> Commands_TodayShowings = new List<string> { "/todayshowings" };
        public static readonly IEnumerable<string> Commands_DailyShowingsAll = new List<string> { "/completeshowings" };
        public static readonly IEnumerable<string> Commands_Films = new List<string> { "/film", "/films" };
        public static readonly IEnumerable<string> Commands_WhichTime = new List<string> { "/ora" };
        public static readonly string CommandsPrefix_FilmDetail = @"/f_";

        public const string Msg_Welcome = "Ciao, sono il bot non ufficiale del The Space di Bologna." +
                                           "Ecco le cose che puoi chiedermi:" +
                                           "\n/aiuto per rileggere questo messaggio' " +
                                           "\n/todayshowings per ricevere la programmazione di oggi' " +
                                           "\n/completeshowings per ricevere la programmazione completa' " +
                                           "\n/film per la lista dei film in programmazione' " +
                                           "\n/film seguito da parte del titolo di un film per conoscerne i dettagli";

        public const string Msg_Error = "Scusa, non ho capito... :sob:" +
                                         "\nPer il momento accetto solo i comandi riportati qui: /aiuto";
    }
}