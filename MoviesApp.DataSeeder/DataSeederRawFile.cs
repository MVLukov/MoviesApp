using Microsoft.EntityFrameworkCore;
using MoviesApp.Data;

namespace MoviesApp.DataSeeder
{
    public class DataSeederRawFile
    {
        private readonly MoviesAppDbContext context;
        private List<string> getGenres = Enum.GetValues(typeof(Genre.Genres)).Cast<Genre.Genres>().Select(x => x.ToString()).ToList();  //list of genres

        public DataSeederRawFile(MoviesAppDbContext context)
        {
            this.context = context;
        }

        public List<MovieRaw> ReadRawFile(string filePath = "./data/MoviesListRaw.txt")
        {
            List<MovieRaw> movieRawList = new List<MovieRaw>();
            MovieRaw movieRaw = new MovieRaw();

            foreach (string line in System.IO.File.ReadLines(filePath))
            {
                string[] lineSplitted = line.Split().ToArray();
                string readLine = "";

                if (lineSplitted[0] == "MovieName:")
                {
                    lineSplitted = lineSplitted.Skip(1).ToArray();
                    readLine = String.Join(" ", lineSplitted);
                    readLine = Filters.movieNameFilter.Replace(readLine, String.Empty);
                    readLine = Filters.singleSpace.Replace(readLine, " ");
                    readLine = readLine.Trim();

                    movieRaw.MovieName = readLine;
                }

                if (lineSplitted[0] == "Director:")
                {
                    lineSplitted = lineSplitted.Skip(1).ToArray();
                    readLine = String.Join(" ", lineSplitted);
                    readLine = Filters.nameFilter.Replace(readLine, String.Empty);
                    readLine = readLine.Trim();

                    movieRaw.Director = readLine;
                }

                if (lineSplitted[0] == "Year:")
                {
                    lineSplitted = lineSplitted.Skip(1).ToArray();
                    readLine = String.Join(" ", lineSplitted);
                    movieRaw.Year = Filters.year.Replace(readLine, String.Empty); ;
                }

                if (lineSplitted[0] == "Genre:")
                {
                    lineSplitted = lineSplitted.Skip(1).ToArray();
                    readLine = String.Join(" ", lineSplitted);
                    readLine = Filters.genre.Replace(readLine, String.Empty);

                    int findGenre = getGenres.FindIndex(x => x.ToLower() == readLine.ToLower());

                    if (findGenre != -1)
                    {
                        movieRaw.Genre = readLine;
                    }
                    else
                    {
                        movieRaw.Genre = "Unknown";
                    }
                }

                if (lineSplitted[0] == "Actors:")
                {
                    lineSplitted = lineSplitted.Skip(1).ToArray();
                    readLine = String.Join(" ", lineSplitted);
                    movieRaw.Actors = readLine.Split("|").Select(x => Filters.nameFilter.Replace(x, String.Empty).Trim()).ToList();
                }

                if (lineSplitted[0] == "Rating:")
                {
                    lineSplitted = lineSplitted.Skip(1).ToArray();
                    readLine = String.Join(" ", lineSplitted);
                    movieRaw.Rating = float.Parse(Filters.rating.Replace(readLine, String.Empty));
                }

                if (lineSplitted[0] == "Synopsis:")
                {
                    lineSplitted = lineSplitted.Skip(1).ToArray();
                    readLine = String.Join(" ", lineSplitted);
                    readLine = Filters.synopsisFilter.Replace(readLine, String.Empty);
                    readLine = Filters.singleSpace.Replace(readLine, " ");
                    readLine = readLine.Trim();

                    if (readLine.Length >= 500)
                    {
                        readLine = readLine.Substring(0, 500);
                    }

                    movieRaw.MovieSynopsis = readLine;
                    movieRawList.Add(movieRaw);

                    movieRaw = new MovieRaw();
                }
            }

            return movieRawList;
        }

        public bool Seed(List<MovieRaw> moviesRawList)
        {
            try
            {
                this.createDb();

                List<MovieRaw> movies = moviesRawList.DistinctBy(x => x.MovieName).ToList();
                List<string> directors = movies.Select(x => x.Director).ToList();
                List<string> actors = new List<string>() { };

                foreach (var item in movies)
                {
                    foreach (var actor in item.Actors)
                    {
                        actors.Add(actor);
                    }
                }

                actors = actors.Distinct().ToList();
                directors = directors.Distinct().ToList();

                foreach (var item in directors)
                {

                    var director = context.Directors.FirstOrDefault(x => x.DirectorName.ToLower() == item.ToLower());

                    if (director == null)
                    {
                        context.Directors.Add(new Director { DirectorName = item });
                    }
                }

                context.SaveChanges();

                foreach (var item in actors)
                {
                    var getActor = context.Actors.FirstOrDefault(x => x.ActorName.ToLower() == item.ToLower());

                    if (getActor == null)
                    {
                        context.Actors.Add(new Actor { ActorName = item });
                    }
                }

                context.SaveChanges();

                foreach (var item in movies)
                {
                    var getMovie = context.Movies.FirstOrDefault(x => x.MovieName.ToLower() == item.MovieName.ToLower());

                    if (getMovie == null)
                    {
                        Movie newMovie = new Movie();

                        newMovie.MovieName = item.MovieName;
                        newMovie.MovieSynopsis = item.MovieSynopsis;
                        newMovie.MovieRating = item.Rating;
                        newMovie.MovieGenre = item.Genre;
                        newMovie.MovieYear = item.Year;

                        var getDirector = context.Directors.FirstOrDefault(x => x.DirectorName.ToLower() == item.Director.ToLower());

                        if (getDirector != null)
                        {
                            newMovie.Director = getDirector;
                        }

                        foreach (var actor in item.Actors)
                        {
                            var getActor = context.Actors.FirstOrDefault(x => x.ActorName.ToLower() == actor.ToLower());

                            if (getActor != null)
                            {
                                newMovie.Actors.Add(getActor);
                            }
                        }
                        context.Movies.Add(newMovie);
                    }
                    else
                    {
                        System.Console.WriteLine($"This movie already exists - {item.MovieName}");
                    }
                }

                context.SaveChanges();

                int movieCount = context.Movies.Count();
                int actorCount = context.Actors.Count();
                int directorsCount = context.Directors.Count();


                System.Console.WriteLine(String.Concat(Enumerable.Repeat("-", 15)));

                System.Console.WriteLine($"Movies read from file: {movies.Count}");
                System.Console.WriteLine($"Actors read from file: {actors.Count}");
                System.Console.WriteLine($"Directors read from file: {directors.Count}");

                System.Console.WriteLine(String.Concat(Enumerable.Repeat("-", 15)));

                System.Console.WriteLine($"Movies in DB - {movieCount}");
                System.Console.WriteLine($"Actors in DB - {actorCount}");
                System.Console.WriteLine($"Directors in DB - {directorsCount}");
                return true;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e);
                return false;
            }
        }

        public void createDb()
        {
            bool canConnect = context.Database.CanConnect();
            if (!canConnect)
            {
                System.Console.WriteLine("Can't connect to DB!");
            }
            else
            {
                context.Database.EnsureCreated();
            }
        }
    }
}