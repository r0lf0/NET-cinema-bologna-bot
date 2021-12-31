using CinemaRolfoBot.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaRolfoBot.Model.Beans
{
    public class UpdateDBOutput
    {
        public List<UpdateShowingsOutput> UpdatedShowings { get; set; }
        public List<Film> AddedFilms { get; set; }
        public List<Film> RemovedFilms { get; set; }

        public UpdateDBOutput()
        {
            this.UpdatedShowings = new List<UpdateShowingsOutput>();
            this.AddedFilms = new List<Film>();
            this.RemovedFilms = new List<Film>();
        }
    }
}