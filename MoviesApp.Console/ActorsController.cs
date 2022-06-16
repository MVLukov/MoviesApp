using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using MoviesApp.Data;
using ConsoleTables;

namespace MoviesApp.Console
{
    public class ActorsController
    {
        private readonly MoviesAppDbContext _context;   //db connection
        public readonly string[] cmdList = new string[] { "help", "ls", "add", "remove", "delete", "update", "edit", "info" };  //list of available commands

        public ActorsController(MoviesAppDbContext context)
        {
            _context = context; //set db connection
        }

        public void cmdActors(string[] cmd)
        {
            if (cmd.Length == 0)
            {
                System.Console.WriteLine($"Sub commands were not specified!");
                return;
            }

            int found = Array.FindIndex(cmdList, x => x == cmd[0]);    //check if cmd exists
            if (found == -1 || cmd[0] == "help") //if cmd is help opr sub cmd was not found
            {
                string cmds = String.Join(", ", cmdList);  //join available commands into string
                System.Console.WriteLine($"{Colors.green}Available commands:{Colors.grey} {cmds}");
                return;
            }

            try
            {
                switch (cmd[0])
                {
                    case "ls": {
                        this.printActors();
                    } break;
                    case "add": {
                        this.addActor();
                    } break;
                    case "remove":
                    case "delete": {
                        this.deleteActor();
                    } break;
                    case "edit":
                    case "update": {
                        this.editActor();
                    } break;
                    case "info": {
                        this.infoActor();
                    } break;
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine($"{Colors.red}Something went wrong!{Colors.grey} {e.Message}");
            }
        }

        private void canConnect()
        {
            if (!_context.Database.CanConnect()) throw new ArgumentException("Can't connect to DB!");    //check DB connection
        }

        private void checkForRecords()
        {
            if (!_context.Actors.Any()) throw new ArgumentException("No records found!");    //check for records
        }

        private void tableActorsSingle(Actor actor)
        {
            var table = new ConsoleTable("ID", "Actor's name"); //create new table object
            table.AddRow(actor.ActorId, actor.ActorName);   //add data to table
            table.Write(Format.MarkDown);   //print table
        }

        private void tableActorsMultiple(List<Actor> actors)
        {
            var table = new ConsoleTable("ID", "Actor's name"); //create new table object
            foreach (var item in actors)
            {
                table.AddRow(item.ActorId, item.ActorName); //add data to table
            }
            table.Write(Format.MarkDown);   //print table
        }

        private void printInfo(Actor actor)
        {
            System.Console.WriteLine($"{Colors.green}ID:{Colors.grey} {actor.ActorId}");
            System.Console.WriteLine($"{Colors.green}Name:{Colors.grey} {actor.ActorName}");

            if (!actor.Movies.Any())
            {
                System.Console.WriteLine($"{Colors.green}Movies:{Colors.grey} This actor doesn't appear in any movie.");
                return;
            }

            System.Console.WriteLine($"{Colors.green}Movies:{Colors.grey} ");
            string[] movies = actor.Movies.Select(x => new String($"[{x.MovieId}] {x.MovieName}")).ToArray();
            foreach (var item in movies)
            {
                System.Console.WriteLine($"\t {item}");
            }
        }

        public void printActors()   //ls command
        {
            this.canConnect();
            this.checkForRecords();

            List<Actor> getActors = _context.Actors.AsNoTracking().ToList();    //get all actors
            this.tableActorsMultiple(getActors);    //print table with actors
        }

        private void addActor()
        {
            this.canConnect();

            System.Console.Write($"{Colors.magenta}Enter actor's name:{Colors.grey} ");
            string actorName = System.Console.ReadLine();    //get input
            actorName = Filter.name.Replace(actorName, String.Empty);  //filter name
            actorName = Filter.singleSpace.Replace(actorName, " ").Trim();    //filter single space; trim

            if (String.IsNullOrEmpty(actorName))
            {
                System.Console.WriteLine($"{Colors.yellow}WARNING: Actors's name was not specified!{Colors.grey}");
                return;
            }

            Actor setActor = new Actor();   //create new actor
            List<ValidationResult> errorResults = new List<ValidationResult>(); //create errors list

            Actor checkActor = _context.Actors.FirstOrDefault(x => x.ActorName.ToLower() == actorName.ToLower());   //check if actor with given name already exists

            setActor.ActorName = actorName; //set actor's name

            if (checkActor != null) //actor not exists
            {
                errorResults.Add(new ValidationResult("This actor already exist!", new string[] { nameof(Actor.ActorName) }));  //set error
            }

            var validateActor = new ValidationContext(setActor, serviceProvider: null, items: null);    //validation object
            var isValid = Validator.TryValidateObject(setActor, validateActor, errorResults, true);     //invoke validation

            if (errorResults.Count != 0) //an error has occurred
            {
                foreach (var item in errorResults)
                {
                    string property = item.MemberNames.First(); //get property name
                    System.Console.WriteLine($"{Colors.red}Property:{Colors.grey} {property} {Colors.red}Error:{Colors.grey} {item.ErrorMessage}");
                }
                return;
            }

            this.tableActorsSingle(setActor);   //print new actor

            System.Console.Write($"{Colors.cyan}Are you sure you want to add this actor? (yes/y or no/n):{Colors.grey} ");
            string confirm = System.Console.ReadLine().Trim().ToLower();    //get input; trim
            confirm = Filter.lettersOnly.Replace(confirm, String.Empty);   //filter only letters

            if (!String.IsNullOrEmpty(confirm) && confirm == "yes" || confirm == "y")
            {
                _context.Actors.Add(setActor);
                _context.SaveChanges();
                System.Console.WriteLine($"{Colors.green}Actor was added successfully!{Colors.grey}");
            }
            else
            {
                System.Console.WriteLine($"{Colors.red}Operation was terminated!{Colors.grey}");
            }
        }

        private void deleteActor()
        {
            this.canConnect();
            this.checkForRecords();
            this.printActors();     //print all actors

            System.Console.Write($"{Colors.magenta}Enter actor's ID:{Colors.grey} ");
            string input = System.Console.ReadLine().Trim();    //get input; trim
            input = Filter.numbersOnly.Replace(input, String.Empty);   //filter numbers only

            if (String.IsNullOrEmpty(input))
            {
                System.Console.WriteLine($"{Colors.yellow}WARNING: Actor's ID was not specified!{Colors.grey}");
                return;
            }

            int actorId = int.Parse(input);     //parse actor id from string to int
            Actor getActor = _context.Actors.FirstOrDefault(x => x.ActorId == actorId);     //check if actor with given id exists
            if (getActor == null)
            {
                System.Console.WriteLine($"{Colors.yellow}WARNING: Actor with ID: {actorId} was not found!{Colors.grey}");
                return;
            }

            this.tableActorsSingle(getActor);   //print found actor

            System.Console.Write($"{Colors.cyan}Are you sure you want to delete this actor? (yes/y or no/n):{Colors.grey} ");
            string confirm = System.Console.ReadLine().Trim().ToLower();    //get input; trim
            confirm = Filter.lettersOnly.Replace(confirm, String.Empty);   //filter letters only

            if (!String.IsNullOrEmpty(confirm) && confirm == "yes" || confirm == "y")
            {
                _context.Actors.Remove(getActor);
                _context.SaveChanges();
                System.Console.WriteLine($"{Colors.green}Actor was deleted successfully!{Colors.grey}");
            }
            else
            {
                System.Console.WriteLine($"{Colors.red}Operation was terminated!{Colors.grey}");
            }
        }

        private void editActor()
        {
            this.canConnect();
            this.checkForRecords();
            this.printActors();     //print all actors

            System.Console.Write($"{Colors.magenta}Enter actor's ID:{Colors.grey} ");
            string input = System.Console.ReadLine().Trim();    //get input; trim
            input = Filter.numbersOnly.Replace(input, String.Empty);   //filter numbers only

            if (String.IsNullOrEmpty(input))
            {
                System.Console.WriteLine($"{Colors.yellow}WARNING: Actor's ID was not specified!{Colors.grey}");
                return;
            }

            int actorId = int.Parse(input);     //parse actor's id from string to int
            Actor getActor = _context.Actors.FirstOrDefault(x => x.ActorId == actorId);     //check if actor with given id exists
            if (getActor == null)
            {
                System.Console.WriteLine($"{Colors.yellow}WARNING: Actor with ID: {actorId} was not found!{Colors.grey}");
                return;
            }

            this.tableActorsSingle(getActor);   //print found record

            System.Console.Write($"{Colors.magenta}Enter new actor's name:{Colors.grey} ");
            string actorName = System.Console.ReadLine();    //get input
            actorName = Filter.name.Replace(actorName, String.Empty);      //filter name
            actorName = Filter.singleSpace.Replace(actorName, " ").Trim();        //filter white spaces; trim

            if (String.IsNullOrEmpty(actorName))
            {
                System.Console.WriteLine($"{Colors.yellow}WARNING: Actor's name was not specified!{Colors.grey}");
                return;
            }

            Actor checkActor = _context.Actors.FirstOrDefault(x => x.ActorName.ToLower() == actorName.ToLower() && x.ActorId != actorId);   //check if actor with given name exists otherwise returns null
            List<ValidationResult> errorResults = new List<ValidationResult>(); //errors list

            if (checkActor == null)
            {
                getActor.ActorName = actorName;     //set new actor's name
            }
            else
            {
                errorResults.Add(new ValidationResult("This actor already exists!", new string[] { nameof(Actor.ActorName) })); //set error
            }

            var validateActor = new ValidationContext(getActor, serviceProvider: null, items: null);    //validation object
            var isValid = Validator.TryValidateObject(getActor, validateActor, errorResults, true);     //invoke validation

            if (errorResults.Count != 0) //an error has occurred
            {
                foreach (var item in errorResults)
                {
                    string property = item.MemberNames.First(); //get property name
                    System.Console.WriteLine($"{Colors.red}Property:{Colors.grey} {property} {Colors.red}Error:{Colors.grey} {item.ErrorMessage}");
                }
                return;
            }

            this.tableActorsSingle(getActor);   //print selected actor with updated value

            System.Console.Write($"{Colors.cyan}Are you sure you want to update this actor? (yes/y or no/n):{Colors.grey} ");
            string confirm = System.Console.ReadLine().Trim().ToLower();    //get input; trim
            confirm = Filter.lettersOnly.Replace(confirm, String.Empty);   //filter letters

            if (!String.IsNullOrEmpty(confirm) && confirm == "yes" || confirm == "y")
            {
                _context.Actors.Update(getActor);
                _context.SaveChanges();
                System.Console.WriteLine($"{Colors.green}Actor was updated successfully!{Colors.grey}");
            }
            else
            {
                System.Console.WriteLine($"{Colors.red}Operation was terminated!{Colors.grey}");
            }
        }

        private void infoActor()
        {
            this.canConnect();
            this.checkForRecords();
            this.printActors();

            System.Console.Write($"{Colors.cyan}Enter actor's id:{Colors.grey} ");
            string input = System.Console.ReadLine();
            input = Filter.numbersOnly.Replace(input, String.Empty);

            if (String.IsNullOrEmpty(input))
            {
                System.Console.WriteLine($"{Colors.yellow}WARNING: Actor's ID was not specified!{Colors.grey}");
                return;
            }

            int actorId = int.Parse(input);
            Actor getActor = _context.Actors.Include(x => x.Movies).AsNoTracking().FirstOrDefault(x => x.ActorId == actorId);
            if (getActor == null)
            {
                System.Console.WriteLine($"{Colors.yellow}WARNING: Actor with ID: {actorId} was not found!{Colors.grey}");
                return;
            }
            this.printInfo(getActor);
        }
    }
}