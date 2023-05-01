using Microsoft.EntityFrameworkCore;

namespace CinemaRolfoBot.Model.DB
{
    public class CinemaContext : DbContext
    {
        private readonly string connectionString;

        public DbSet<Film> Films { get; set; }
        public DbSet<Showing> Showings { get; set; }

        public DbSet<RunningInfo> RunningInfos { get; set; }

        public CinemaContext(string connectionString)
        {
            this.connectionString = connectionString;
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}