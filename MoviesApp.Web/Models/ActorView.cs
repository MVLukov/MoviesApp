using MoviesApp.Data;

namespace MoviesApp.Models
{
    public class ActorView
    {
        public int ActorId { get; set; }
        public string ActorName { get; set; }
        public ICollection<Movie> Movies { get; set; } = new List<Movie>();

        public ActorView()
        {

        }

        public ActorView(int id, string name, List<Movie> movies)
        {
            ActorId = id;
            ActorName = name;
            Movies = movies;
        }
    }
}