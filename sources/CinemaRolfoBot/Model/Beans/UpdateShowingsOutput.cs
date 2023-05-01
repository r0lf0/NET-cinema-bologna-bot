using CinemaRolfoBot.Model.DB;

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