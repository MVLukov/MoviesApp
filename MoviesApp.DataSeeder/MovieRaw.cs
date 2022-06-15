namespace MoviesApp.DataSeeder
{
    public class MovieRaw
    {
        public string MovieName { get; set; }
        public string MovieSynopsis { get; set; }
        public string Director { get; set; }
        public string Year { get; set; }
        public float Rating { get; set; }
        public string Genre { get; set; }
        public List<string> Actors { get; set; } = new List<string>();

        public MovieRaw()
        {
            
        }
    }
}