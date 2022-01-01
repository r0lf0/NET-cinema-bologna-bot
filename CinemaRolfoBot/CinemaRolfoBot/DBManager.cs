using CinemaRolfoBot.Model.Beans;
using CinemaRolfoBot.Model.DB;
using CinemaRolfoBot.Model.Json;
using CinemaRolfoBot.Utils;
using log4net;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CinemaRolfoBot
{
    public class DbManager
    {
        private readonly CinemaContext Context;

        private static readonly HttpClient httpClient = new HttpClient();
        private static readonly ILog Log = LogManager.GetLogger(typeof(DbManager));

        public DbManager(string PostgresConnectionString)
        {
            Context = new CinemaContext(PostgresConnectionString);
        }

        public string UpdateDB(string lastFilmsWithShowing, out UpdateDBOutput output)
        {
            output = new UpdateDBOutput();
            string responseBody = "";
            try
            {
                responseBody = AsyncHelpers.RunSync(() => httpClient.GetStringAsync(Const.TSB_FILMS_WITH_SHOWING_URL));
            }
            catch (HttpRequestException e)
            {
                //TODO Notify errors to mantainers users
                Log.Error($"HTTP error on getting '{Const.TSB_FILMS_WITH_SHOWING_URL}'. Error message: {e.Message}");
                return lastFilmsWithShowing;
            }

            //Comparing with previous responseBody
            string currentFilmsWithShowings = Regex.Replace(responseBody, @"\s+", string.Empty);
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
                Log.Error($"JSON parsing error while analyzing '{Const.TSB_FILMS_WITH_SHOWING_URL}' response. Error message: {e.Message}");
                return lastFilmsWithShowing;
            }

            //Populating DB
            if (filmsWithShowings != null)
            {
                //Updating "lastUpdate" info on DB
                var lastUpdate = Context.RunningInfos.Find(ERunningInfoId.LastUpdate);
                if (lastUpdate == null)
                {
                    lastUpdate = new RunningInfo(ERunningInfoId.LastUpdate, DateTime.Now);
                    Context.RunningInfos.Add(lastUpdate);
                }
                else
                    lastUpdate.Value = DateTime.Now;

                //Check for new or modified films
                foreach (Model.Json.Film? filmJson in filmsWithShowings.films ?? Enumerable.Empty<Model.Json.Film>())
                {
                    Model.DB.Film? filmDB = Context.Films.Include(f => f.Showings).FirstOrDefault(f => f.Id == filmJson.id);
                    if (filmDB == null) //New film to be added
                    {
                        UpdateShowingsOutput updateShowingsOutput = new UpdateShowingsOutput();
                        filmDB = new Model.DB.Film(filmJson, out updateShowingsOutput);
                        Context.Films.Add(filmDB);

                        output.AddedFilms.Add(filmDB);
                        output.UpdatedShowings.Add(updateShowingsOutput);
                    }
                    else //Modified film
                    {
                        output.UpdatedShowings.Add(filmDB.UpdateFilm(filmJson));
                    }
                }

                //Check for removed films
                foreach (Model.DB.Film filmDb in Context.Films.Include(f => f.Showings))
                {
                    Model.Json.Film? filmJson = filmsWithShowings.films.FirstOrDefault(f => f.id == filmDb.Id);
                    if (filmJson == null) //Removed film
                        output.RemovedFilms.Add(filmDb);
                }

                //Remove films from DB
                foreach (Model.DB.Film filmToRemove in output.RemovedFilms)
                    Context.Remove(filmToRemove);

                try
                {
                    Context.SaveChanges();
                }
                catch (Exception e)
                {
                    ;
                }
            }

            return currentFilmsWithShowings;
        }

        public void WipeDB()
        {
            foreach (Model.DB.Film film in Context.Films)
            {
                foreach (Model.DB.Showing showing in film.Showings ?? Enumerable.Empty<Model.DB.Showing>())
                    Context.Showings.Remove(showing);
                Context.Films.Remove(film);
            }
            Context.SaveChanges();
        }
    }
}