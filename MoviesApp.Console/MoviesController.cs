using Microsoft.EntityFrameworkCore;
using MoviesApp.Data;
using ConsoleTables;
using System.ComponentModel.DataAnnotations;

namespace MoviesApp.Console
{
    public class MoviesController
    {
        private readonly MoviesAppDbContext _context;   //db connection
        private readonly ActorsController _actorCtl;    //actor controller instance
        private readonly DirectorsCollector _directorCtl;   //director controller instance
        private List<string> getGenres = Enum.GetValues(typeof(Genre.Genres)).Cast<Genre.Genres>().Select(x => x.ToString()).ToList();  //list of genres
        public string[] cmdsList = new string[] { "help", "ls", "add", "remove", "delete", "update", "edit", "filter", "sort", "info" };    //list of all availabe cmds

        public MoviesController(MoviesAppDbContext context, ActorsController actorCtl, DirectorsCollector directorCtl)
        {
            _context = context;     //set db connection
            _actorCtl = actorCtl;   //set actor controller instance
            _directorCtl = directorCtl; //set director controller instance
        }

        public void cmdMovies(string[] cmd)
        {
            if (cmd.Length >= 1) //check if cmd includes sum commands
            {
                int found = Array.FindIndex(cmdsList, x => x == cmd[0]);    //check if cmd exists

                if (found == -1 || cmd[0] == "help")    //if cmd is help or sub cmd was not found
                {
                    string cmds = String.Join(", ", cmdsList);  //join available commands into string
                    System.Console.WriteLine($"{Colors.green}Available commands:{Colors.grey} {cmds}");
                    System.Console.WriteLine($"{Colors.green}Filter:{Colors.grey} name - asc/desc | genre - asc/desc | rating - asc/desc | year - asc/desc");
                }

                if (cmd[0] == "ls")
                {
                    this.printMovies(cmd);  //print all movies
                }

                if (cmd[0] == "add")
                {
                    this.addMovie();    //add new movie
                }

                if (cmd[0] == "remove" || cmd[0] == "delete")
                {
                    this.deleteMovie(); //delete movie
                }

                if (cmd[0] == "edit" || cmd[0] == "update")
                {
                    this.editMovie();   //edit movie
                }

                if (cmd[0] == "filter" || cmd[0] == "sort")
                {
                    this.filterMovie(cmd.Skip(1).ToArray());  //sort movies
                }

                if (cmd[0] == "info")
                {
                    this.infoMovie();   //show info about movie
                }
            }
            else
            {
                System.Console.WriteLine($"{Colors.yellow}WARNING: Sub commands are not specified!{Colors.grey}");
            }
        }
        private void canConnect()
        {
            if (!_context.Database.CanConnect()) throw new ArgumentException("Can't connect to DB!");    //check DB conn
        }

        private void checkForRecords()
        {
            if (!_context.Movies.Any()) throw new ArgumentException("No records found!");   //check for existring movies
        }

        private void tableMovieSingle(Movie movie)
        {
            string actors = "Not specified!";
            if (movie.Actors.Count > 0)
            {
                actors = String.Join(", ", movie.Actors.Select(x => new String($"[{x.ActorId}]{x.ActorName}")));    //join all actors into one string
            }

            string director = "Not specified!";
            if (movie.DirectorId != null)
            {
                director = $"[{movie.DirectorId}]{movie.Director.DirectorName}";
            }

            var table = new ConsoleTable("ID", "Movie's name", "Movie's synopsis", "Movie's genre", "Movie's rating", "Movie's year", "Movie's director", "Movie's actors");    //create new table object

            string[] synopsis = movie.MovieSynopsis.Split(" ");
            if (synopsis.Length > 10)
            {
                synopsis[10] = "...";
                table.AddRow(movie.MovieId, movie.MovieName, String.Join(" ", synopsis.Take(11)), movie.MovieGenre, movie.MovieRating, movie.MovieYear, director, actors);  //add data to table
            }
            else
            {
                table.AddRow(movie.MovieId, movie.MovieName, movie.MovieSynopsis, movie.MovieGenre, movie.MovieRating, movie.MovieYear, director, actors);  //add data to table
            }

            table.Write(Format.MarkDown);   //print table
        }

        private void tableMoviesMultiple(List<Movie> movies, bool flag = false)
        {
            if (flag)   //if short table is selected
            {
                var table = new ConsoleTable("ID", "Movie's name", "Movie's synopsis", "Movie's genre", "Movie's rating", "Movie's year");  //create new table object

                foreach (var item in movies)
                {
                    string[] synopsis = item.MovieSynopsis.Split(" ");
                    if (synopsis.Length > 10)
                    {
                        synopsis[10] = "...";
                        table.AddRow(item.MovieId, item.MovieName, String.Join(" ", synopsis.Take(11)), item.MovieGenre, item.MovieRating, item.MovieYear);    //add data to table
                    }
                    else
                    {
                        table.AddRow(item.MovieId, item.MovieName, item.MovieSynopsis, item.MovieGenre, item.MovieRating, item.MovieYear);    //add data to table
                    }

                }
                table.Write(Format.MarkDown);   //print table
            }
            else
            {
                var table = new ConsoleTable("ID", "Movie's name", "Movie's synopsis", "Movie's genre", "Movie's rating", "Movie's year", "Movie's director", "Movie's actors");    //create new table object

                foreach (var item in movies)
                {
                    string actors = "Not specified!";
                    if (item.Actors.Count > 0)
                    {
                        actors = String.Join(", ", item.Actors.Select(x => new String($"[{x.ActorId}]{x.ActorName}"))); //join all actors into string
                    }

                    string director = "Not specified!";
                    if (item.DirectorId != null)
                    {
                        director = $"[{item.Director.DirectorId}]{item.Director.DirectorName}";
                    }

                    string[] synopsis = item.MovieSynopsis.Split(" ");
                    if (synopsis.Length > 10)
                    {
                        synopsis[10] = "...";
                        table.AddRow(item.MovieId, item.MovieName, String.Join(" ", synopsis.Take(11)), item.MovieGenre, item.MovieRating, item.MovieYear, director, actors);    //add data to table
                    }
                    else
                    {
                        table.AddRow(item.MovieId, item.MovieName, item.MovieSynopsis, item.MovieGenre, item.MovieRating, item.MovieYear, director, actors);    //add data to table
                    }


                }
                table.Write(Format.MarkDown);   //print table
            }
        }

        private void printInfo(Movie movie)
        {
            System.Console.WriteLine($"{Colors.green}ID:{Colors.grey} {movie.MovieId}");
            System.Console.WriteLine($"{Colors.green}Name:{Colors.grey} {movie.MovieName}");
            System.Console.WriteLine($"{Colors.green}Synopsis:{Colors.grey} {movie.MovieSynopsis}");
            System.Console.WriteLine($"{Colors.green}Genre:{Colors.grey} {movie.MovieGenre}");
            System.Console.WriteLine($"{Colors.green}Year:{Colors.grey} {movie.MovieYear}");
            System.Console.WriteLine($"{Colors.green}Rating:{Colors.grey} {movie.MovieRating}");
            System.Console.WriteLine($"{Colors.green}Director ID:{Colors.grey} {movie.DirectorId}");
            System.Console.WriteLine($"{Colors.green}Director:{Colors.grey} {movie.Director.DirectorName}");

            if (movie.Actors.Any())
            {
                string[] actors = movie.Actors.Select(x => new String($"[{x.ActorId}] {x.ActorName}")).ToArray();

                System.Console.WriteLine($"{Colors.green}Actors:{Colors.grey} ");
                foreach (var item in actors)
                {
                    System.Console.WriteLine($"\t {item}");
                }
            }
            else
            {
                System.Console.WriteLine($"{Colors.green}Actors:{Colors.grey} Actors are not specified!");
            }
        }

        private void printMovies(string[] cmd)
        {
            try
            {
                this.canConnect();
                this.checkForRecords();

                if (cmd.Length > 1 && cmd[1] == "short")
                {
                    List<Movie> getMovies = _context.Movies.OrderBy(x => x.MovieId).AsNoTracking().ToList(); //get all movies sorted by genre
                    this.tableMoviesMultiple(getMovies, true);  //print short table
                }
                else
                {
                    List<Movie> getMovies = _context.Movies.Include(x => x.Actors).Include(x => x.Director).OrderBy(x => x.MovieId).AsNoTracking().ToList(); //get all movies including actors and directors; sort by genre
                    this.tableMoviesMultiple(getMovies);    //print default table
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"{Colors.red}Something went wrong!{Colors.grey} {e.Message}");
            }
        }

        private void addMovie()
        {
            try
            {
                this.canConnect();

                Movie newMovie = new Movie();   //new movie
                List<ValidationResult> errorResults = new List<ValidationResult>(); //create error list

                //movie's name
                System.Console.Write($"{Colors.magenta}Movie's name:{Colors.grey} ");
                string movieName = System.Console.ReadLine();    //get input;
                movieName = Filter.movieName.Replace(movieName, String.Empty);      //filter name
                movieName = Filter.singleSpace.Replace(movieName, " ").Trim();        //filter single space; trim

                Movie getMovie = _context.Movies.AsNoTracking().FirstOrDefault(x => x.MovieName.ToLower() == movieName.ToLower()); //check if movie with entered movieName exists

                if (getMovie == null)
                {
                    newMovie.MovieName = movieName; //set movie's name
                }
                else
                {
                    errorResults.Add(new ValidationResult("This movie already exists!", new string[] { nameof(Movie.MovieName) })); //add error
                }

                //movie's synopsis
                System.Console.Write($"{Colors.magenta}Movie's synopsis:{Colors.grey} ");
                string movieSynopsis = System.Console.ReadLine();    //get input
                movieSynopsis = Filter.movieSynopsis.Replace(movieSynopsis, String.Empty);  //filter synopsis
                movieSynopsis = Filter.singleSpace.Replace(movieSynopsis, " ").Trim();    //filter single space

                if (!String.IsNullOrEmpty(movieSynopsis))
                {
                    if (movieSynopsis.Length >= 500)
                    {
                        movieSynopsis = movieSynopsis.Substring(0, 500);
                    }
                }

                newMovie.MovieSynopsis = movieSynopsis;     //set movie's synopsis

                //movie's year
                System.Console.Write($"{Colors.magenta}Movie's year:{Colors.grey} ");
                string movieYear = System.Console.ReadLine().Trim();    //get input; trim
                movieYear = Filter.numbersOnly.Replace(movieYear, string.Empty);   //filter numbers only
                newMovie.MovieYear = movieYear;     //set movie's year

                //movie's genre
                string printGenres = String.Join(", ", getGenres);  //join genres into a string to print it in console
                System.Console.WriteLine($"Movie genres: {printGenres}");
                System.Console.Write($"{Colors.magenta}Movie's genre:{Colors.grey} ");
                string movieGenre = System.Console.ReadLine().Trim().ToLower(); //get input; trim; to lower case
                movieGenre = Filter.lettersOnly.Replace(movieGenre, String.Empty); //filter letters only

                if (!String.IsNullOrEmpty(movieGenre))
                {
                    string findGenre = getGenres.FirstOrDefault(x => x.ToLower() == movieGenre);    //check if genre exist

                    if (!String.IsNullOrEmpty(findGenre))
                    {
                        newMovie.MovieGenre = findGenre;    //set movie's genre
                    }
                    else
                    {
                        System.Console.WriteLine("Genre is set to Unknown.");
                        newMovie.MovieGenre = "Unknown";
                    }
                }
                else
                {
                    System.Console.WriteLine("Genre set to Unknown.");
                    newMovie.MovieGenre = "Unknown";
                }

                //movie's rating
                System.Console.Write($"{Colors.magenta}Movie's rating:{Colors.grey} ");
                string movieRatingRaw = System.Console.ReadLine().Trim();   //get input; trim
                movieRatingRaw = Filter.rating.Replace(movieRatingRaw, String.Empty); //filter numbers only

                if (!String.IsNullOrEmpty(movieRatingRaw))
                {
                    float movieRating = float.Parse(movieRatingRaw);    //convert movie's rating to float
                    newMovie.MovieRating = movieRating;     //set movie's rating
                }
                else
                {
                    System.Console.WriteLine("Rating set to 0.");
                    newMovie.MovieRating = 0;
                }

                //movie's director
                _directorCtl.printDirectors();  //print all directors
                System.Console.Write($"{Colors.magenta}Movie's director (by ID):{Colors.grey} ");
                string movieDirectorIdRaw = System.Console.ReadLine().Trim(); //get input; trim
                movieDirectorIdRaw = Filter.numbersOnly.Replace(movieDirectorIdRaw, String.Empty); //filter numbers only

                if (!String.IsNullOrEmpty(movieDirectorIdRaw))
                {
                    int movieDirectorId = int.Parse(movieDirectorIdRaw);    //convert movie's director id to int

                    Director getDirector = _context.Directors.FirstOrDefault(x => x.DirectorId == movieDirectorId);
                    if (getDirector != null)
                    {
                        newMovie.Director = getDirector;    //set movie's director
                        newMovie.DirectorId = movieDirectorId;    //set director id
                    }
                    else
                    {
                        System.Console.WriteLine($"{Colors.yellow}WARNING: Director with ID: {movieDirectorId} wasn't found!{Colors.grey}");
                    }
                }

                //movie's actors
                _actorCtl.printActors();    //print all actors
                System.Console.Write($"{Colors.magenta}Movie's actors (by ID):{Colors.grey} ");
                List<int> movieActors = System.Console.ReadLine().Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => //INPUT separated with ;
                {
                    x = Filter.numbersOnly.Replace(x, String.Empty);   //filter numbers only
                    if (String.IsNullOrEmpty(x)) return 0;  //return 0 if string is empty/null
                    return int.Parse(x);    //parse actor's id to int
                }).Distinct().ToList();     //make unique; parse to list

                List<Actor> getActors = new List<Actor>();
                foreach (var item in movieActors)
                {
                    Actor getActor = _context.Actors.FirstOrDefault(x => x.ActorId == item);    //check if actor exist

                    if (getActor != null)
                    {
                        getActors.Add(getActor);    //add actor to movie
                    }
                    else
                    {
                        System.Console.WriteLine($"{Colors.yellow}WARNING: Actor with ID: {item} wasn't found!{Colors.grey}");
                    }
                }
                newMovie.Actors = getActors;    //set actors

                var validateMovie = new ValidationContext(newMovie, serviceProvider: null, items: null);    //validation object
                var isValid = Validator.TryValidateObject(newMovie, validateMovie, errorResults, true);     //invoke validation

                if (errorResults.Count == 0)    //no errors
                {
                    this.tableMovieSingle(newMovie);   //print new movie

                    System.Console.Write($"{Colors.cyan}Are you sure you want to add this movie? (yes/y or no/n):{Colors.grey} ");
                    string confirm = System.Console.ReadLine().Trim().ToLower(); //get input; trim; to lower case   
                    confirm = Filter.lettersOnly.Replace(confirm, String.Empty);   //filter letters only

                    if (!String.IsNullOrEmpty(confirm) && confirm == "yes" || confirm == "y")
                    {
                        _context.Movies.Add(newMovie);
                        _context.SaveChanges();
                        System.Console.WriteLine($"{Colors.green}Movie was added successfully!{Colors.grey}");
                    }
                    else
                    {
                        System.Console.WriteLine($"{Colors.red}Operation was terminated!{Colors.grey}");
                    }
                }
                else
                {
                    foreach (var item in errorResults)
                    {
                        string property = item.MemberNames.First(); //get property name
                        System.Console.WriteLine($"{Colors.red}Property:{Colors.grey} {property}  {Colors.red}Error:{Colors.grey} {item.ErrorMessage}");
                    }
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"{Colors.red}Something went wrong!{Colors.grey} {e.Message}");
            }
        }

        private void deleteMovie()
        {
            try
            {
                this.canConnect();
                this.printMovies(new string[] { "ls", "short" }); //print all movies; parse empty arr to print long version of movies table

                System.Console.Write($"{Colors.magenta}Enter movie's ID:{Colors.grey} ");
                string input = System.Console.ReadLine().Trim();    //get input; trim
                input = Filter.numbersOnly.Replace(input, String.Empty);   //filter numbers only

                if (!String.IsNullOrEmpty(input))
                {
                    int movieId = int.Parse(input);     //converts movie's id to int

                    Movie getMovie = _context.Movies.Include(x => x.Actors).Include(x => x.Director).FirstOrDefault(x => x.MovieId == movieId); //check if movie exists

                    if (getMovie != null)
                    {
                        this.tableMovieSingle(getMovie);   //print selected movie

                        System.Console.Write($"{Colors.cyan}Are you sure you want to delete this movie? (yes/y or no/n):{Colors.grey} ");
                        input = System.Console.ReadLine().Trim().ToLower();     //get input; trim
                        input = Filter.lettersOnly.Replace(input, String.Empty);   //filter letters only

                        if (!String.IsNullOrEmpty(input) && input == "yes" || input == "y")
                        {
                            _context.Movies.Remove(getMovie);
                            _context.SaveChanges();
                            System.Console.WriteLine($"{Colors.green}Movie was deleted successfully!{Colors.grey}");
                        }
                        else
                        {
                            System.Console.WriteLine($"{Colors.red}Operation was terminated!{Colors.grey}");
                        }
                    }
                    else
                    {
                        System.Console.WriteLine($"{Colors.yellow}WARNING: Movie with ID: {movieId} was not found!{Colors.grey}");
                    }
                }
                else
                {
                    System.Console.WriteLine($"{Colors.yellow}WARNING: Movie's ID was not specified!{Colors.grey}");
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"{Colors.red}Something went wrong!{Colors.grey} {e.Message}");
            }
        }

        private void editMovie()
        {
            try
            {
                this.canConnect();

                this.printMovies(new string[] { "ls", "short" }); //print all movies
                List<ValidationResult> errorResults = new List<ValidationResult>(); //errors list

                System.Console.Write($"{Colors.magenta}Enter movie's ID to edit:{Colors.grey} ");
                string input = System.Console.ReadLine().Trim();    //get input; trim
                input = Filter.numbersOnly.Replace(input, string.Empty);   //filter numbers only

                if (!String.IsNullOrEmpty(input))
                {
                    int movieId = int.Parse(input); //convert movie's id to int
                    Movie getMovie = _context.Movies.Include(x => x.Actors).Include(x => x.Director).FirstOrDefault(x => x.MovieId == movieId);   //find movie

                    if (getMovie != null)
                    {
                        this.tableMovieSingle(getMovie);    //print selected movie

                        //movie's name
                        System.Console.Write($"{Colors.magenta}Edit movie's name:{Colors.grey} ");
                        string movieName = System.Console.ReadLine();    //get input
                        movieName = Filter.movieName.Replace(movieName, String.Empty);  //filter name
                        movieName = Filter.singleSpace.Replace(movieName, " ").Trim();    //filter single space; trim

                        if (!String.IsNullOrEmpty(movieName))
                        {
                            Movie checkMovie = _context.Movies.FirstOrDefault(x => x.MovieName.ToLower() == movieName.ToLower() && x.MovieId != movieId);   //check if movie with entered movieName exists

                            if (checkMovie == null)   //if movie with given name doesn't exist or is the same movie with the same name
                            {
                                getMovie.MovieName = movieName;     //set movie's name
                            }
                            else
                            {
                                errorResults.Add(new ValidationResult($"This movie already exists!", new string[] { nameof(Movie.MovieName) }));    //set error
                            }
                        }

                        //movie's synopsis
                        System.Console.Write($"{Colors.magenta}Edit movie's synopsis:{Colors.grey} ");
                        string movieSynopsis = System.Console.ReadLine();    //get input
                        movieSynopsis = Filter.movieSynopsis.Replace(movieSynopsis, String.Empty);  //filter synopsis
                        movieSynopsis = Filter.singleSpace.Replace(movieSynopsis, " ").Trim();    //filter single space; trim

                        if (!String.IsNullOrEmpty(movieSynopsis))
                        {
                            if (movieSynopsis.Length >= 500)
                            {
                                movieSynopsis = movieSynopsis.Substring(0, 500);
                            }

                            getMovie.MovieSynopsis = movieSynopsis;     //set movie's synopsis
                        }

                        //movie's year
                        System.Console.Write($"{Colors.magenta}Edit movie's year:{Colors.grey} ");
                        string movieYear = System.Console.ReadLine().Trim();    //get input; trim
                        movieYear = Filter.numbersOnly.Replace(movieYear, string.Empty);   //filter numbers only

                        if (!String.IsNullOrEmpty(movieYear))
                        {
                            getMovie.MovieYear = movieYear;     //set movie's year
                        }

                        //movie's genre
                        string printGenres = String.Join(", ", getGenres);  //join genres into a string to print it in console
                        System.Console.WriteLine($"Movie genres: {printGenres}");
                        System.Console.Write($"{Colors.magenta}Edit movie's genre:{Colors.grey} ");
                        string movieGenre = System.Console.ReadLine().Trim().ToLower(); //get input; trim; to lower case
                        movieGenre = Filter.lettersOnly.Replace(movieGenre, String.Empty); //filter letters only

                        if (!String.IsNullOrEmpty(movieGenre))
                        {
                            string findGenre = getGenres.FirstOrDefault(x => x.ToLower() == movieGenre);    //check if entered genre exists

                            if (!String.IsNullOrEmpty(findGenre))
                            {
                                getMovie.MovieGenre = findGenre;    //set movie's genre
                            }
                            else
                            {
                                errorResults.Add(new ValidationResult("Genre not found!", new string[] { nameof(Movie.MovieGenre) }));  //set error
                            }
                        }

                        //movie's rating
                        System.Console.Write($"{Colors.magenta}Edit movies's rating:{Colors.grey} ");
                        string movieRatingRaw = System.Console.ReadLine().Trim();   //get input; trim
                        movieRatingRaw = Filter.rating.Replace(movieRatingRaw, String.Empty); //filter numbers only

                        if (!String.IsNullOrEmpty(movieRatingRaw))
                        {
                            float movieRating = float.Parse(movieRatingRaw);    //convert movie's rating to float
                            getMovie.MovieRating = movieRating;     //set movie's rating
                        }

                        //movie's director
                        _directorCtl.printDirectors();  //print all directors
                        System.Console.Write($"{Colors.magenta}Edit movies's director (by ID):{Colors.grey} ");
                        string movieDirectorRaw = System.Console.ReadLine().Trim(); //get input; trim
                        movieDirectorRaw = Filter.numbersOnly.Replace(movieDirectorRaw, String.Empty); //filter numbers only

                        if (!String.IsNullOrEmpty(movieDirectorRaw))
                        {
                            int directorId = int.Parse(movieDirectorRaw);    //convert director's id to int

                            Director getDirector = _context.Directors.FirstOrDefault(x => x.DirectorId == directorId);   //check if director exists

                            if (getDirector != null)
                            {
                                getMovie.Director = getDirector;    //set movie's director
                                getMovie.DirectorId = directorId;   //set movie's director id
                            }
                            else
                            {
                                System.Console.WriteLine($"{Colors.yellow}WARNING: Director with ID: {directorId} was'n found!{Colors.grey}");
                            }
                        }

                        //movie actors
                        _actorCtl.printActors();    //print all actors
                        System.Console.Write($"{Colors.magenta}Edit movie's actors (by ID):{Colors.grey} ");
                        List<int> movieActors = System.Console.ReadLine().Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => //get input separeted with ;
                        {
                            x = Filter.numbersOnly.Replace(x, String.Empty);   //regex numbers only
                            if (String.IsNullOrEmpty(x)) return 0;  //return 0 if input is empty/null
                            return int.Parse(x);    //convert actors id to int
                        }).Distinct().ToList();     //make unique; parse to list

                        List<Actor> getActors = new List<Actor>();  //list of actors
                        foreach (var item in movieActors)
                        {
                            Actor getActor = _context.Actors.FirstOrDefault(x => x.ActorId == item);    //check if actor exists

                            if (getActor != null)
                            {
                                getActors.Add(getActor);    //add actor
                            }
                            else
                            {
                                System.Console.WriteLine($"{Colors.yellow}WARNING: Actor with ID: {item} wasn't found!{Colors.grey}");
                            }
                        }

                        if (getActors.Count > 0)
                        {
                            getMovie.Actors = getActors;    //set new actors
                        }

                        var validateMovie = new ValidationContext(getMovie, serviceProvider: null, items: null);    //validation object
                        var isValid = Validator.TryValidateObject(getMovie, validateMovie, errorResults, true);     //invoke validation

                        if (errorResults.Count == 0)
                        {
                            this.tableMovieSingle(getMovie);   //print selected movie

                            System.Console.Write($"{Colors.cyan}Are you sure you want to update the movie? (yes/y or no/n):{Colors.grey} ");
                            string confirm = System.Console.ReadLine().ToLower(); //get input; trim; to lower case
                            confirm = Filter.lettersOnly.Replace(confirm, String.Empty);   //filter letters only

                            if (!String.IsNullOrEmpty(confirm) && confirm == "yes" || confirm == "y")
                            {
                                _context.Movies.Update(getMovie);
                                _context.SaveChanges();
                                System.Console.WriteLine($"{Colors.green}Movie was updated successfully!{Colors.grey}");
                            }
                            else
                            {
                                System.Console.WriteLine($"{Colors.red}Operation was terminated!{Colors.grey}");
                            }
                        }
                        else
                        {
                            foreach (var item in errorResults)
                            {
                                string property = item.MemberNames.First(); //get property name
                                System.Console.WriteLine($"{Colors.red}Property:{Colors.grey} {property}  {Colors.red}Error:{Colors.grey} {item.ErrorMessage}");
                            }
                        }
                    }
                    else
                    {
                        System.Console.WriteLine($"{Colors.yellow}WARNING: Movie with ID: {input} was not found!{Colors.grey}");
                    }
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"{Colors.red}Something went wrong!{Colors.grey} {e}");
            }
        }

        private void filterMovie(string[] cmds)
        {
            try
            {
                this.canConnect();

                if (cmds.Length < 2)
                {
                    this.cmdMovies(new string[] { "help" });  //execute help command
                    return;
                }

                string filter = cmds[0];
                string orderType = cmds[1];

                if (filter == "genre")
                {
                    if (orderType == "asc")
                    {
                        List<Movie> getMovies = _context.Movies.Include(x => x.Actors).Include(x => x.Director).OrderBy(x => x.MovieGenre).AsNoTracking().ToList();
                        this.tableMoviesMultiple(getMovies, true);
                    }
                    else if (orderType == "desc")
                    {
                        List<Movie> getMovies = _context.Movies.Include(x => x.Actors).Include(x => x.Director).OrderByDescending(x => x.MovieGenre).AsNoTracking().ToList();
                        this.tableMoviesMultiple(getMovies, true);
                    }
                    else
                    {
                        System.Console.WriteLine("Filter not found!");
                        this.cmdMovies(new string[] { "help" });  //execute help command
                    }
                }
                else if (filter == "name")
                {
                    if (orderType == "asc")
                    {
                        List<Movie> getMovies = _context.Movies.Include(x => x.Actors).Include(x => x.Director).OrderBy(x => x.MovieName).AsNoTracking().ToList();
                        this.tableMoviesMultiple(getMovies, true);
                    }
                    else if (orderType == "desc")
                    {
                        List<Movie> getMovies = _context.Movies.Include(x => x.Actors).Include(x => x.Director).OrderByDescending(x => x.MovieName).AsNoTracking().ToList();
                        this.tableMoviesMultiple(getMovies, true);
                    }
                    else
                    {
                        System.Console.WriteLine("Filter not found!");
                        this.cmdMovies(new string[] { "help" });  //execute help command
                    }
                }
                else if (filter == "rating")
                {
                    if (orderType == "asc")
                    {
                        List<Movie> getMovies = _context.Movies.Include(x => x.Actors).Include(x => x.Director).OrderBy(x => x.MovieRating).AsNoTracking().ToList();
                        this.tableMoviesMultiple(getMovies, true);
                    }
                    else if (orderType == "desc")
                    {
                        List<Movie> getMovies = _context.Movies.Include(x => x.Actors).Include(x => x.Director).OrderByDescending(x => x.MovieRating).AsNoTracking().ToList();
                        this.tableMoviesMultiple(getMovies, true);
                    }
                    else
                    {
                        System.Console.WriteLine("Filter not found!");
                        this.cmdMovies(new string[] { "help" });  //execute help command
                    }
                }
                else if (filter == "year")
                {
                    if (orderType == "asc")
                    {
                        List<Movie> getMovies = _context.Movies.Include(x => x.Actors).Include(x => x.Director).OrderBy(x => x.MovieYear).AsNoTracking().ToList();
                        this.tableMoviesMultiple(getMovies, true);
                    }
                    else if (orderType == "desc")
                    {
                        List<Movie> getMovies = _context.Movies.Include(x => x.Actors).Include(x => x.Director).OrderByDescending(x => x.MovieYear).AsNoTracking().ToList();
                        this.tableMoviesMultiple(getMovies, true);
                    }
                    else
                    {
                        System.Console.WriteLine("Filter not found!");
                        this.cmdMovies(new string[] { "help" });  //execute help command
                    }
                }
                else
                {
                    System.Console.WriteLine("Filter not found!");
                    this.cmdMovies(new string[] { "help" });  //execute help command
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"{Colors.red}Something went wrong!{Colors.grey} {e.Message}");
            }
        }
        private void infoMovie()
        {
            try
            {
                this.printMovies(new string[] { "ls", "short" });     //invoke print cmd

                System.Console.Write($"{Colors.cyan}Enter movie's id:{Colors.grey} ");
                string input = System.Console.ReadLine();
                input = Filter.numbersOnly.Replace(input, String.Empty).Trim();     //filter numbers only; trim

                if (!String.IsNullOrEmpty(input))
                {
                    int movieId = int.Parse(input);     //prase movie id to int

                    Movie getMovie = _context.Movies.Include(x => x.Actors).Include(x => x.Director).OrderBy(x => x.MovieId).AsNoTracking().FirstOrDefault(x => x.MovieId == movieId);      //get movie with given id

                    if (getMovie != null)   //if movie exists
                    {
                        this.printInfo(getMovie);   //print the movie
                    }
                    else
                    {
                        System.Console.WriteLine($"{Colors.red}Movie was not found!{Colors.grey}");
                    }
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"{Colors.red}Something went wrong!{Colors.grey} {e.Message}");
            }
        }
    }
}