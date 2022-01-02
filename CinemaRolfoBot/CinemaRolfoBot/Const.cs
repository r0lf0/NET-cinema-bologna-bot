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
    }
}