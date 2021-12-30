using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaRolfoBot.Model.DB
{
    public class Film
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public string? Title { get; set; }
        public DateTime? Released { get; set; }
        public string? Director { get; set; }
        public string? Cast { get; set; }
        public string? Synopsis { get; set; }
        public string? RunningTime { get; set; }
        public string? TrailerLink { get; set; }
        public string? PosterLink { get; set; }
        public byte[]? Poster { get; set; }
        public string[]? Genres { get; set; }

        public ICollection<Showing>? Showings { get; set; }
    }
}