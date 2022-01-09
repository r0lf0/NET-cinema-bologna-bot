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

        private string lastFilmsWithShowings = "";

        public DbManager(string PostgresConnectionString)
        {
            Context = new CinemaContext(PostgresConnectionString);
        }

        public void UpdateDB(string filmsWithShowings_json, out UpdateDBOutput output)
        {
            output = new UpdateDBOutput();

            //Comparing with previous responseBody
            if (filmsWithShowings_json == lastFilmsWithShowings)
                return;

            FilmsWithShowings? filmsWithShowings = null;
            try
            {
                filmsWithShowings = JsonSerializer.Deserialize<FilmsWithShowings>(filmsWithShowings_json);
            }
            catch (JsonException e)
            {
                //TODO Notify errors to mantainers users
                Log.Error($"JSON parsing error while analyzing '{Const.TSB_FILMS_WITH_SHOWING_URL}' response. Error message: {e.Message}");
                return;
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
                    output = new UpdateDBOutput();
                    Log.Error($"Error while updating database.");
                }
            }

            return;
        }

        public void WipeDB()
        {
            RunningInfo? lastReset = Context.RunningInfos.Find(ERunningInfoId.LastReset);
            if (lastReset == null)
                Context.RunningInfos.Add(new RunningInfo(ERunningInfoId.LastReset, DateTime.Now));
            else
                lastReset.Value = DateTime.Now;

            foreach (Model.DB.Film film in Context.Films)
            {
                foreach (Model.DB.Showing showing in film.Showings ?? Enumerable.Empty<Model.DB.Showing>())
                    Context.Showings.Remove(showing);
                Context.Films.Remove(film);
            }

            Context.SaveChanges();
        }

        public DateTime? GetRunningInfo(ERunningInfoId runningInfoId)
        {
            return (Context?.RunningInfos?.Find(runningInfoId)?.Value);
        }

        public Model.DB.Film? GetFilmDetail(string filmId)
        {
            return Context?.Films?.Include(f => f.Showings).FirstOrDefault(f => f.Id == filmId);
        }

        public IEnumerable<Model.DB.Film> GetAllFilms(bool withShowings)
        {
            if (withShowings)
                return Context?.Films?.Include(f => f.Showings);
            else
                return Context?.Films;
        }
    }
}