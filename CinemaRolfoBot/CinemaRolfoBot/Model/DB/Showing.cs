using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaRolfoBot.Model.DB
{
    public class Showing
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public DateTime DateAndTime { get; set; }

        public int Screen { get; set; }
    }
}