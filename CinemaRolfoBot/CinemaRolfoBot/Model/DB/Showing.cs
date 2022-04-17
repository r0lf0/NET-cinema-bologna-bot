using CinemaRolfoBot.Utils;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaRolfoBot.Model.DB
{
    public class Showing
    {
        [Key]
        [Required]
        public string Id { get; set; }

        [Required]
        public DateTime DateAndTime
        {
            get => _DateAndTime;
            set { _DateAndTime = value.SetKindUtc(); }
        }

        [NotMapped]
        private DateTime _DateAndTime;

        public string Screen { get; set; }

        //Foreign key for Film
        [Required]
        public string FilmId { get; set; }

        public Film Standard { get; set; }
        public static Showing Where { get; internal set; }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != typeof(Showing))
                return base.Equals(obj);

            Showing typedObj = (Showing)obj;
            return (this.Id == typedObj.Id
                && this.DateAndTime == typedObj.DateAndTime
                && this.Screen == typedObj.Screen);
        }

        public Showing()
        {
        }

        public Showing(Json.Time timeJson)
        {
            this.Id = timeJson.session_id;
            this.DateAndTime = timeJson.date;
            this.Screen = timeJson.screen_number;
        }
    }
}