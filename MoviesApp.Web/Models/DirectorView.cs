using MoviesApp.Data;

namespace MoviesApp.Models
{
    public class DirectorView
    {
        public int DirectorId { get; set; }
        public string DirectorName { get; set; }
        public ICollection<Movie> Movies { get; set; } = new List<Movie>();

        public DirectorView()
        {

        }

        public DirectorView(int id, string name, List<Movie> movies)
        {
            DirectorId = id;
            DirectorName = name;
            Movies = movies;
        }
    }
}