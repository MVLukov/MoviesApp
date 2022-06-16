using MoviesApp.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using MoviesApp.DataSeeder;

namespace MoviesApp.Console
{
    public class Program
    {
        private static IServiceProvider serviceProvider;

        public static void Main(string[] args)
        {
            DotNetEnv.Env.TraversePath().Load();
            string[] mainCmd = new string[] { "movie", "director", "actor" };

            ConfigureServices();
            var context = serviceProvider.GetService<MoviesAppDbContext>();

            string db_seed = Environment.GetEnvironmentVariable("db_seed");
            var seed = new DataSeederRawFile(context);

            if (db_seed == "true")
            {
                var result = seed.ReadRawFile();
                seed.Seed(result);
            }
            else
            {
                seed.createDb();
            }

            context = serviceProvider.GetService<MoviesAppDbContext>();

            ActorsController actorCtl = new ActorsController(context);
            DirectorsCollector directorCtl = new DirectorsCollector(context);
            MoviesController movieCtl = new MoviesController(context, actorCtl, directorCtl);
            while (true)
            {
                System.Console.Write($"{Colors.cyan}Enter cmd:{Colors.grey} ");
                string[] input = readConsole();

                if (input.Length == 0)
                    continue;

                if (input[0] == "exit")
                    break;

                // Too lazy to actually fix it lol
                int found = Array.FindIndex(mainCmd, x => x == input[0]);
                if (found == -1) input[0] = "help";

                switch (input[0])
                {
                    case "help": {
                        string moviesCmds = String.Join(", ", movieCtl.cmdsList);
                        string directorsCmds = String.Join(", ", directorCtl.cmdsList);
                        string actorsCmd = String.Join(", ", actorCtl.cmdList);

                        System.Console.WriteLine($"movie - {moviesCmds}");
                        System.Console.WriteLine($"actor - {actorsCmd}");
                        System.Console.WriteLine($"director - {directorsCmds}");
                    } break;
                    case "movie": {
                        input = input.Skip(1).ToArray();
                        movieCtl.cmdMovies(input);
                    } break;
                    case "actor": {
                        input = input.Skip(1).ToArray();
                        actorCtl.cmdActors(input);
                    } break;
                    case "director": {
                        input = input.Skip(1).ToArray();
                        directorCtl.cmdDirectors(input);
                    } break;
                }
            }
            // This won't be called all the time. For example if the user presses Ctrl+C.
            // We hope that either the runtime or the OS will free the memory lol.
            context.Dispose();
        }

        public static string[] readConsole()
        {
            string[] input = System.Console.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(x => Regex.Replace(x.ToLower(), "[^a-zA-Z]", String.Empty)).ToArray();
            return input;
        }

        private static void ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddDbContext<MoviesAppDbContext>(options =>
            {
                string db_username = Environment.GetEnvironmentVariable("db_username");
                string db_password = Environment.GetEnvironmentVariable("db_password");
                string db_host = Environment.GetEnvironmentVariable("db_host");
                string db_name = Environment.GetEnvironmentVariable("db_name");

                string dbcs = $"Server={db_host};Database={db_name};Uid={db_username};Pwd={db_password};";
                options.UseMySql(
                    connectionString: dbcs,
                    new MySqlServerVersion(new Version(8, 0, 29))
                );
            });

            serviceProvider = services.BuildServiceProvider();
        }
    }
}