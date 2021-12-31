using CinemaRolfoBot.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaRolfoBot.Model.Beans
{
    public class UpdateShowingsOutput
    {
        public Film Film;

        public List<Showing> AddedShowings { get; set; }
        public List<Showing> ModifiedShowings { get; set; }
        public List<Showing> RemovedShowings { get; set; }

        public UpdateShowingsOutput()
        {
            this.AddedShowings = new List<Showing>();
            this.ModifiedShowings = new List<Showing>();
            this.RemovedShowings = new List<Showing>();
        }
    }
}