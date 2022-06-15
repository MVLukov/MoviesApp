using Microsoft.EntityFrameworkCore;
using MoviesApp.Data;
using ConsoleTables;
using System.ComponentModel.DataAnnotations;

namespace MoviesApp.Console
{
    public class DirectorsCollector
    {
        private readonly MoviesAppDbContext _context;   //db connection
        public string[] cmdsList = new string[] { "help", "ls", "add", "remove", "delete", "update", "edit", "info" };  //all available commands

        public DirectorsCollector(MoviesAppDbContext context)
        {
            _context = context; //set db connection
        }

        public void cmdDirectors(string[] cmd)
        {
            if (cmd.Length >= 1) //if cmd include sub commands
            {
                int found = Array.FindIndex(cmdsList, x => x == cmd[0]);    //check if cmd exists

                if (found == -1 || cmd[0] == "help")    //if cmd is help or sub cmd was not found
                {
                    string cmds = String.Join(", ", cmdsList);  //join available commands into string
                    System.Console.WriteLine($"{Colors.green}Available commands:{Colors.grey} {cmds}");
                }

                if (cmd[0] == "ls")
                {
                    this.printDirectors();  //print all directors
                }

                if (cmd[0] == "add")
                {
                    this.addDirector();     //add new director
                }

                if (cmd[0] == "remove" || cmd[0] == "delete")
                {
                    this.deleteDirector();  //remove director
                }

                if (cmd[0] == "edit" || cmd[0] == "update")
                {
                    this.editDirector();    //edit director
                }

                if (cmd[0] == "info")
                {
                    this.infoDirector();    //print more info
                }
            }
            else
            {
                System.Console.WriteLine($"{Colors.yellow}Sub commands were not specified!{Colors.grey}");
            }
        }

        private void canConnect()
        {
            if (!_context.Database.CanConnect()) throw new ArgumentException("Can't connect to DB!");   //check DB connection
        }

        private void checkForRecords()
        {
            if (!_context.Directors.Any()) throw new ArgumentException("No records found!");    //check for records
        }

        private void tableDirectorSingle(Director director)
        {
            var table = new ConsoleTable("ID", "Director's name");  //create new table object
            table.AddRow(director.DirectorId, director.DirectorName);   //add data to table
            table.Write(Format.MarkDown);   //print table
        }

        private void tableDirectorsMultiple(List<Director> directors)
        {
            var table = new ConsoleTable("ID", "Director's name");  //create new table object
            foreach (var item in directors)
            {
                table.AddRow(item.DirectorId, item.DirectorName);   //add data to table
            }
            table.Write(Format.MarkDown);   //print table
        }

        private void printInfo(Director director, List<Movie> movies)
        {
            System.Console.WriteLine($"{Colors.green}ID:{Colors.grey} {director.DirectorId}");
            System.Console.WriteLine($"{Colors.green}Name:{Colors.grey} {director.DirectorName}");

            if (movies.Any())
            {
                System.Console.WriteLine($"{Colors.green}Movies:{Colors.grey} ");
                foreach (var item in movies)
                {
                    System.Console.WriteLine($"\t [{item.MovieId}] {item.MovieName}");
                }
            }
            else
            {
                System.Console.WriteLine($"{Colors.green}Movies:{Colors.grey} This director doesn't appear in any movie.");
            }
        }

        public void printDirectors()
        {
            try
            {
                this.canConnect();
                this.checkForRecords();

                List<Director> getDirectors = _context.Directors.AsNoTracking().ToList();   //get all directors
                tableDirectorsMultiple(getDirectors);   //print table with directors
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"{Colors.red}Something went wrong!{Colors.grey} {e.Message}");
            }
        }

        private void addDirector()
        {
            try
            {
                this.canConnect();

                System.Console.Write($"{Colors.magenta}Enter director's name: {Colors.grey}");
                string input = System.Console.ReadLine();    //get input; trim
                input = Filter.name.Replace(input, String.Empty);  //filter name
                input = Filter.singleSpace.Replace(input, " ").Trim();    //filter single space

                if (!String.IsNullOrEmpty(input))
                {
                    Director setDirector = new Director();  //create new director
                    List<ValidationResult> errorResults = new List<ValidationResult>(); //create errors list

                    Director checkDirector = _context.Directors.FirstOrDefault(x => x.DirectorName.ToLower() == input.ToLower()); //check if director with given name already exists

                    if (checkDirector == null)  //director not exists
                    {
                        setDirector.DirectorName = input;   //set director's name
                    }
                    else
                    {
                        errorResults.Add(new ValidationResult("This director already exists!", new string[] { nameof(Director.DirectorName) }));    //set error
                    }

                    var validateDirector = new ValidationContext(setDirector, serviceProvider: null, items: null);  //validation object
                    var isValid = Validator.TryValidateObject(setDirector, validateDirector, errorResults, true);   //invoke validation

                    if (errorResults.Count == 0)    //no errors
                    {
                        this.tableDirectorSingle(setDirector);  //print new director

                        System.Console.Write($"{Colors.cyan}Are you sure you want to add this director? (yes/y or no/n):{Colors.grey} ");
                        string confirm = System.Console.ReadLine().Trim().ToLower();    //get input; trim
                        confirm = Filter.lettersOnly.Replace(confirm, String.Empty);   //filter letters only

                        if (!String.IsNullOrEmpty(confirm) && confirm == "yes" || confirm == "y")
                        {
                            _context.Directors.Add(setDirector);
                            _context.SaveChanges();
                            System.Console.WriteLine($"{Colors.green}Director was added successfully!{Colors.grey}");
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
                            System.Console.WriteLine($"{Colors.red}Property:{Colors.grey} {property} {Colors.red}Error:{Colors.grey} {item.ErrorMessage}");
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"{Colors.red}Something went wrong!{Colors.grey} {e.Message}");
            }
        }

        private void deleteDirector()
        {
            try
            {
                this.canConnect();
                this.checkForRecords();
                this.printDirectors();  //print all directors

                System.Console.Write($"{Colors.magenta}Enter director's ID: {Colors.grey}");
                string input = System.Console.ReadLine().Trim();    //input; trim
                input = Filter.numbersOnly.Replace(input, String.Empty);   //filter numbers only

                if (!String.IsNullOrEmpty(input))
                {
                    int directorId = int.Parse(input);  //convert director's id to int

                    Director getDirector = _context.Directors.FirstOrDefault(x => x.DirectorId == directorId);  //check if director with given id exists

                    if (getDirector != null)    //directors exists
                    {
                        this.tableDirectorSingle(getDirector);  //print selected director

                        System.Console.Write($"{Colors.cyan}Are you sure you want to delete this director? (yes/y or no/n):{Colors.grey} ");
                        string confirm = System.Console.ReadLine().Trim().ToLower();    //input; trim; lower case
                        confirm = Filter.lettersOnly.Replace(confirm, String.Empty);   //filter letters only

                        if (!String.IsNullOrEmpty(confirm) && confirm == "yes" || confirm == "y")
                        {
                            List<Movie> getMovies = _context.Movies.Where(x => x.DirectorId == directorId).ToList();    //get all movies with selected director
                            getMovies.Select(x => x.DirectorId = null); //remove director's id for selected movies

                            _context.Directors.Remove(getDirector);
                            _context.SaveChanges();
                            System.Console.WriteLine($"{Colors.green}Director was deleted successfully!{Colors.grey}");
                        }
                        else
                        {
                            System.Console.WriteLine($"{Colors.red}Operation was terminated!{Colors.grey}");
                        }
                    }
                    else
                    {
                        System.Console.WriteLine($"{Colors.yellow}WARNING: Director with ID: {directorId} was not found!{Colors.grey}");
                    }
                }
                else
                {
                    System.Console.WriteLine($"{Colors.yellow}WARNING: Director's ID was not specified!{Colors.grey}");
                }

            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"{Colors.red}Something went wrong!{Colors.grey} {e.Message}");
            }
        }

        private void editDirector()
        {
            try
            {
                this.canConnect();
                this.checkForRecords();
                this.printDirectors();  //print all directors

                System.Console.Write($"{Colors.magenta}Enter director's ID:{Colors.grey} ");
                string input = System.Console.ReadLine().Trim();    //get input; trim
                input = Filter.numbersOnly.Replace(input, String.Empty);   //filter numbers only

                if (!String.IsNullOrEmpty(input))
                {
                    int directorId = int.Parse(input);  //convert director's id to int
                    List<ValidationResult> errorResults = new List<ValidationResult>(); //create errors list

                    Director getDirector = _context.Directors.FirstOrDefault(x => x.DirectorId == directorId);  //check if director with given id exists

                    if (getDirector != null)
                    {
                        this.tableDirectorSingle(getDirector);  //print found director

                        System.Console.Write($"{Colors.magenta}Enter new director's name:{Colors.grey} ");
                        string directorName = System.Console.ReadLine();     //get input; trim
                        directorName = Filter.name.Replace(directorName, String.Empty);    //filter name
                        directorName = Filter.singleSpace.Replace(directorName, " ").Trim();      //filter single space

                        if (!String.IsNullOrEmpty(directorName))
                        {
                            Director checkDirector = _context.Directors.FirstOrDefault(x => x.DirectorName.ToLower() == directorName.ToLower() && x.DirectorId != directorId);    //check if director with given name exists

                            getDirector.DirectorName = directorName; //set director's name
                            if (checkDirector != null)
                            {
                                errorResults.Add(new ValidationResult("This director already exists!", new string[] { nameof(Director.DirectorName) }));
                            }

                            var validateDirector = new ValidationContext(getDirector, serviceProvider: null, items: null);  //validation object
                            var isValid = Validator.TryValidateObject(getDirector, validateDirector, errorResults, true);   //invoke validation

                            if (errorResults.Count == 0)    //no errors
                            {
                                this.tableDirectorSingle(getDirector);  //print selected director

                                System.Console.Write($"{Colors.cyan}Are you sure you want to update this director? (yes/y or no/n):{Colors.grey} ");
                                string confirm = System.Console.ReadLine().Trim().ToLower();    //input; trim; lower case
                                confirm = Filter.lettersOnly.Replace(confirm, String.Empty);   //filter letters only

                                if (!String.IsNullOrEmpty(confirm) && confirm == "yes" || confirm == "y")
                                {
                                    _context.Directors.Update(getDirector);
                                    _context.SaveChanges();
                                    System.Console.WriteLine($"{Colors.green}Director was updated successfully!{Colors.grey}");
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
                                    string property = item.MemberNames.First();     //get property name
                                    System.Console.WriteLine($"{Colors.red}Property:{Colors.grey} {property} {Colors.red}Error:{Colors.grey} {item.ErrorMessage}");
                                }
                            }
                        }
                        else
                        {
                            System.Console.WriteLine($"{Colors.yellow}WARNING: Director's name was not specified!{Colors.grey}");
                        }
                    }
                    else
                    {
                        System.Console.WriteLine($"{Colors.yellow}WARNING: Director with ID: {directorId} was not found!{Colors.grey}");
                    }
                }
                else
                {
                    System.Console.WriteLine($"{Colors.yellow}WARNING: Director's ID was not specified!{Colors.grey}");
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"{Colors.red}Something went wrong!{Colors.grey} {e.Message}");
            }
        }

        private void infoDirector()
        {
            try
            {
                this.printDirectors();

                System.Console.Write($"{Colors.cyan}Enter director's ID:{Colors.grey} ");
                string input = System.Console.ReadLine();
                input = Filter.numbersOnly.Replace(input, String.Empty);

                if (!String.IsNullOrEmpty(input))
                {
                    int directorId = int.Parse(input);
                    Director getDirector = _context.Directors.AsNoTracking().FirstOrDefault(x => x.DirectorId == directorId);

                    if (getDirector != null)
                    {
                        List<Movie> getMovies = _context.Movies.Where(x => x.DirectorId == directorId).ToList();

                        this.printInfo(getDirector, getMovies);
                    }
                    else
                    {
                        System.Console.WriteLine($"{Colors.yellow}WARNING: Director with ID: {directorId} was not found!{Colors.grey}");
                    }
                }
                else
                {
                    System.Console.WriteLine($"{Colors.yellow}WARNING: Director's ID was not specified!{Colors.grey}");
                }
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"{Colors.red}Something went wrong!{Colors.grey} {e.Message}");
            }
        }
    }
}