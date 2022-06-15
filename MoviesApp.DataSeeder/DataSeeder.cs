using System.Text.Json;
using MoviesApp.Data;

namespace MoviesApp.DataSeeder
{
    public class Seeder
    {
        private readonly MoviesAppDbContext context;
        public Seeder(MoviesAppDbContext context)
        {
            this.context = context;
        }

        public void Seed()
        {
            bool canConnect = context.Database.CanConnect();
            if (!canConnect)
            {
                System.Console.WriteLine("Can't connect to DB!");
                return;
            }
            else
            {
                context.Database.EnsureCreated();
            }

            string actorsJsonFilePath = "Actors.json";
            string actorsJson = File.ReadAllText(actorsJsonFilePath);

            string directorsJsonFilePath = "Directors.json";
            string directorsJson = File.ReadAllText(directorsJsonFilePath);

            string moviesJsonFilePath = "Movies.json";
            string moviesJson = File.ReadAllText(moviesJsonFilePath);

            string actorMovieJsonFilePath = "ActorMovie.json";
            string actorMovieJson = File.ReadAllText(actorMovieJsonFilePath);

            List<Actor> actorsJsonList = JsonSerializer.Deserialize<List<Actor>>(actorsJson)!;
            List<Director> directorJsonsList = JsonSerializer.Deserialize<List<Director>>(directorsJson)!;
            List<Movie> movieJsonsList = JsonSerializer.Deserialize<List<Movie>>(moviesJson)!;
            List<ActorMovie> actorMovieJsonList = JsonSerializer.Deserialize<List<ActorMovie>>(actorMovieJson)!;

            System.Console.WriteLine(String.Concat(Enumerable.Repeat("-", 15)));
            System.Console.WriteLine($"Actors count: {actorsJsonList.Count}");
            System.Console.WriteLine($"Directors count: {directorJsonsList.Count}");
            System.Console.WriteLine($"Movies count: {movieJsonsList.Count}");
            System.Console.WriteLine(String.Concat(Enumerable.Repeat("-", 15)));
            System.Console.WriteLine();

            if (!context.Actors.Any() && !context.Directors.Any() && !context.Movies.Any())
            {
                context.Actors.AddRange(actorsJsonList);
                context.Directors.AddRange(directorJsonsList);
                context.Movies.AddRange(movieJsonsList);
                context.SaveChanges();

                foreach (var item in actorMovieJsonList)
                {
                    Actor getActor = context.Actors.FirstOrDefault(x => x.ActorId == item.ActorsActorId);
                    Movie getMovie = context.Movies.FirstOrDefault(x => x.MovieId == item.MoviesMovieId);

                    if (getActor != null && getMovie != null)
                    {
                        getMovie.Actors.Add(getActor);
                    }
                }
                context.SaveChanges();
            }

            List<Actor> actors = context.Actors.ToList();
            List<Director> directors = context.Directors.ToList();
            List<Movie> movies = context.Movies.ToList();

            System.Console.WriteLine(String.Concat(Enumerable.Repeat("-", 15)));
            System.Console.WriteLine($"Actors count in DB: {actors.Count}");
            System.Console.WriteLine($"Directors count in DB: {directors.Count}");
            System.Console.WriteLine($"Movies count in DB: {movies.Count}");
            System.Console.WriteLine(String.Concat(Enumerable.Repeat("-", 15)));
        }
    }
}