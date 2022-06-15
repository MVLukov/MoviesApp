using MoviesApp.Data;

namespace MoviesApp.Models
{
    public class MovieView
    {
        public int MovieId { get; set; }
        public string MovieName { get; set; }
        public string MovieSynopsis { get; set; }
        public string MovieGenre { get; set; }
        public string MovieYear { get; set; }
        public float MovieRating { get; set; }
        public Director Director { get; set; }
        public ICollection<Actor> Actors { get; set; } = new List<Actor>();

        public MovieView()
        {

        }

        public MovieView(int id, string name, string synopsis, string genre, string year, float rating, Director director, List<Actor> actors)
        {
            MovieId = id;
            MovieName = name;
            MovieSynopsis = synopsis;
            MovieGenre = genre;
            MovieYear = year;
            MovieRating = rating;
            Director = director;
            Actors = actors;
        }
    }
}