using CinemaRolfoBot.Utils;
using System.Globalization;
using CinemaRolfoBot.Model.DB;

namespace CinemaRolfoBot
{
    public static class ShowingParser
    {
        private static CultureInfo Culture = new System.Globalization.CultureInfo("it-IT");

        /// <summary>
        /// Get parsed informations about a given <paramref name="film"/>
        /// </summary>
        /// <param name="film">Film to get informations about</param>
        /// <returns>Parsed informations</returns>
        public static string ParseFilmDetails(Model.DB.Film film)
        {
            string parsedFilm = "";
            parsedFilm += $"*{BotMessagesUtils.TelegramStringEscape(film.Title)}*\n";
            if (film.Genres?.Any() == true)
            {
                parsedFilm += "Genere: ";
                foreach (string genre in film.Genres)
                    parsedFilm += $"{genre}, ";
                parsedFilm = parsedFilm.Remove(parsedFilm.Length - 2);
            }
            if (film.Released.HasValue)
                parsedFilm += $"Data di uscita: {film.Released.Value.ToString("dd/MM/yyyy")}\n";
            if (!string.IsNullOrWhiteSpace(film.Director))
                parsedFilm += $"Regia: {BotMessagesUtils.TelegramStringEscape(film.Director)}\n";
            if (!string.IsNullOrWhiteSpace(film.Cast))
                parsedFilm += $"Cast: {BotMessagesUtils.TelegramStringEscape(film.Cast)}\n";
            if (!string.IsNullOrWhiteSpace(film.RunningTime))
                parsedFilm += $"Durata: {BotMessagesUtils.TelegramStringEscape(film.RunningTime)}\n";
            if (!string.IsNullOrWhiteSpace(film.Synopsis))
                parsedFilm += $"_{BotMessagesUtils.TelegramStringEscape(film.Synopsis)}_\n";
            if (!string.IsNullOrWhiteSpace(film.TrailerLink))
                parsedFilm += $"[Visualizza trailer]({BotMessagesUtils.TelegramStringEscape(film.TrailerLink)})\n";

            return parsedFilm;
        }

        /// <summary>
        /// Get parsed showings of a given <paramref name="film"/>
        /// </summary>
        /// <param name="film">Film to get showings of</param>
        /// <returns>Parsed showings</returns>
        public static string ParseFilmShowings(Model.DB.Film film)
        {
            if (film.Showings?.Any() != true)
                return "_Nessuna proiezione in programma_";

            string parsedShowings = "*Programmazione*\n";
            DateTime lastDate = DateTime.MinValue;
            foreach (Model.DB.Showing? showing in film.Showings.OrderBy(s => s.DateAndTime))
            {
                if (showing.DateAndTime.Date >= lastDate.AddDays(1))
                    parsedShowings += $":calendar:Spettacoli di {Culture.DateTimeFormat.GetDayName(showing.DateAndTime.DayOfWeek)} {showing.DateAndTime.ToString("dd/MM")}\n";
                parsedShowings += BotMessagesUtils.TelegramStringEscape($"{showing.DateAndTime.ToString("HH:ss")} - sala {showing.Screen}\n");
                lastDate = showing.DateAndTime.Date;
            }

            return parsedShowings;
        }

        /// <summary>
        /// Get parsed <paramref name="films"/> list
        /// </summary>
        /// <param name="films">Given films list</param>
        /// <returns>Parsed <paramref name="films"/> list</returns>
        public static string ParseFilmsList(IEnumerable<Model.DB.Film> films)
        {
            if (films == null || films.Count() <= 0)
                return "_Nessun film in programmazione_";

            string output = ":film_frames: *Film in programmazione️* :film_frames:\n\n";

            DateTime lastDate = DateTime.MinValue;
            foreach (Model.DB.Film film in films.OrderBy(f => f.Released ?? DateTime.MinValue))
            {
                if (film.Released >= lastDate.AddDays(1) && DateTime.Now < film.Released)
                    output += $"\n:calendar:In uscita il {film.Released.Value.ToString("dd/MM/yyyy")}\n";

                output += BotMessagesUtils.TelegramStringEscape($"{Const.CommandsPrefix_FilmDetail}{film.Id} {film.Title}\n");
                lastDate = film.Released ?? DateTime.MaxValue;
            }

            output += "\n";

            return output;
        }

        /// <summary>
        /// Get parsed today showing list from <paramref name="film"/> collection
        /// </summary>
        /// <param name="films">Complete films list</param>
        /// <returns>Parsed today showings list</returns>
        public static string ParseTodayShowingsList(IEnumerable<Model.DB.Film> films)
        {
            string output = ":film_frames: *Film in programmazione️ oggi " + DateTime.Today.Day + "/" + DateTime.Today.Month + "* :film_frames:\n";
            foreach (Model.DB.Film film in films.OrderBy(f => f.Released ?? DateTime.MaxValue))
            {
                bool writtenFilmTitle = false;
                foreach (Showing showing in film.Showings)
                {
                    if (showing.DateAndTime.Date == DateTime.Today.Date && showing.DateAndTime.AddMinutes(10) >= DateTime.Today)
                    {
                        if (!writtenFilmTitle)
                        {
                            output += "\n" + BotMessagesUtils.TelegramStringEscape(film.Title) + "\n";
                            writtenFilmTitle = true;
                        }
                        output += "\\- " + showing.DateAndTime.Hour + ":" + showing.DateAndTime.Minute;
                        if (showing.DateAndTime.Minute == 0)
                            output += "0";
                        output += " sala " + showing.Screen + "\n";
                    }
                }
            }
            return output;
        }
    }
}