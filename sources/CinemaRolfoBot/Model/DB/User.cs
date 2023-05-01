using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaRolfoBot.Model.DB
{
    public enum EUserRole
    {
        Administrator,
        Subscriber
    }

    public class User
    {
        [Key]
        [Required]
        public long Id { get; set; }

        [Required]
        public EUserRole Role { get; set; }
    }
}