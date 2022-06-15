using Microsoft.EntityFrameworkCore;

namespace MoviesApp.Data
{
    public class MoviesAppDbContext : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Director> Directors { get; set; }

        public MoviesAppDbContext(DbContextOptions options) : base(options)
        {

        }
    }
}