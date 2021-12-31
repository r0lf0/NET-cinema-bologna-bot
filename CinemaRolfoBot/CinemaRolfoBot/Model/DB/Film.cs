using CinemaRolfoBot.Model.Beans;
using CinemaRolfoBot.Model.Json;
using CinemaRolfoBot.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CinemaRolfoBot.Model.DB
{
    public class Film
    {
        private static readonly HttpClient httpClient = new HttpClient();

        [Key]
        [Required]
        public string Id { get; set; }

        public string? Title { get; set; }

        public DateTime? Released
        {
            get => _Released;
            set { _Released = value.SetKindUtc(); }
        }

        [NotMapped]
        private DateTime? _Released;

        public string? Director { get; set; }
        public string? Cast { get; set; }
        public string? Synopsis { get; set; }
        public string? RunningTime { get; set; }
        public string? TrailerLink { get; set; }
        public string? PosterLink { get; set; }
        public byte[]? Poster { get; set; }
        public List<string> Genres { get; set; }

        public ICollection<Showing> Showings { get; set; }

        public Film()
        {
            this.Genres = new List<string>();
            this.Showings = new List<Showing>();
        }

        public Film(Json.Film filmJson, out UpdateShowingsOutput output)
        {
            this.Id = filmJson.id;
            this.Title = filmJson.title;
            this.Released = filmJson.ReleaseDate.SetKindUtc();
            this.Director = filmJson.info_director;
            this.Cast = filmJson.info_cast;
            this.Synopsis = filmJson.synopsis_short;
            this.RunningTime = filmJson.info_runningtime;
            this.TrailerLink = filmJson.video;
            this.PosterLink = filmJson.image_poster;

            this.Genres = new List<string>();
            foreach (Name genreJson in filmJson?.genres?.names ?? Enumerable.Empty<Name>())
            {
                if (!string.IsNullOrWhiteSpace(genreJson?.name))
                    this.Genres.Add(genreJson.name);
            }

            //Downloading poster image
            this.DownloadPoster();

            this.Showings = new List<Showing>();
            foreach (Json.Showing showingJson in filmJson?.showings?.AsEnumerable() ?? Enumerable.Empty<Json.Showing>())
            {
                foreach (Time timeJson in showingJson?.times?.AsEnumerable() ?? Enumerable.Empty<Time>())
                {
                    this.Showings.Add(new Showing(timeJson));
                }
            }

            output = UpdateShowings(filmJson?.showings?.SelectMany(s => s.times));
        }

        /// <summary>
        /// Update Film infos and showings
        /// </summary>
        /// <param name="filmJson"></param>
        /// <returns>Modified showings</returns>
        public UpdateShowingsOutput UpdateFilm(Json.Film filmJson)
        {
            this.Title = filmJson.title ?? this.Title;
            this.Released = filmJson.ReleaseDate ?? this.Released;
            this.Director = filmJson.info_director ?? this.Director;
            this.Cast = filmJson.info_cast ?? this.Cast;
            this.Synopsis = filmJson.synopsis_short ?? this.Synopsis;
            this.RunningTime = filmJson.info_runningtime ?? this.RunningTime;
            this.TrailerLink = filmJson.video ?? this.TrailerLink;
            this.PosterLink = filmJson.image_poster ?? this.PosterLink;
            this.DownloadPoster();

            if (filmJson?.genres?.names?.Any() == true)
            {
                this.Genres = new List<string>();
                foreach (Name genreJson in filmJson?.genres?.names ?? Enumerable.Empty<Name>())
                {
                    if (!string.IsNullOrWhiteSpace(genreJson?.name))
                        this.Genres.Add(genreJson.name);
                }
            }

            return UpdateShowings(filmJson.showings.SelectMany(s => s.times));
        }

        private void DownloadPoster()
        {
            if (Uri.TryCreate(this.PosterLink, UriKind.Absolute, out Uri? uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                try
                {
                    this.Poster = AsyncHelpers.RunSync(() => httpClient.GetByteArrayAsync(uriResult));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while downloading poster for film {this.Id}-{this.Title}. Message: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Update the showings list
        /// </summary>
        /// <param name="timesJson"></param>
        /// <returns></returns>
        public UpdateShowingsOutput UpdateShowings(IEnumerable<Json.Time>? timesJson)
        {
            UpdateShowingsOutput output = new();
            output.Film = this;

            if (timesJson == null)
                timesJson = Enumerable.Empty<Json.Time>();
            if (this.Showings == null)
                this.Showings = new List<Showing>();

            foreach (Json.Time timeJson in timesJson)
            {
                Showing showingDb_current = this.Showings.FirstOrDefault(s => s.Id == timeJson.session_id);
                Showing showingDb_new = new Showing(timeJson);

                if (showingDb_current == null) //New showing added
                {
                    this.Showings.Add(showingDb_new);
                    output.AddedShowings.Add(showingDb_new);
                }
                else if (!showingDb_new.Equals(showingDb_current)) //Modified showing
                {
                    this.Showings.Remove(showingDb_current);
                    this.Showings.Add(showingDb_new);
                    output.ModifiedShowings.Add(showingDb_new);
                }
            }
            //Check for removed showings
            foreach (Showing showingDB in this.Showings)
            {
                Json.Time? timeJson = timesJson.FirstOrDefault(t => t.session_id == showingDB.Id);
                if (timeJson == null) //Removed showing
                    output.RemovedShowings.Add(showingDB);
            }

            foreach (Showing showingToRemove in output.RemovedShowings ?? Enumerable.Empty<Showing>())
                this.Showings.Remove(showingToRemove);

            return output;
        }
    }
}