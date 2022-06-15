using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MoviesApp.Data;

namespace MoviesApp.DataSeeder
{
    public static class Program
    {
        private static IServiceProvider serviceProvider;

        public static void Main(string[] args)
        {
            ConfigureServices();
            var context = serviceProvider.GetService<MoviesAppDbContext>();

            //import from json files
            // Seeder seeder = new Seeder(context);
            // seeder.Seed();

            //import from txt file
            string rawFilePath = "./data/MoviesListRaw.txt";
            DataSeederRawFile seederRaw = new DataSeederRawFile(context);
            var result = seederRaw.ReadRawFile(rawFilePath);

            seederRaw.Seed(result);
        }

        private static void ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddDbContext<MoviesAppDbContext>(options =>
            {
                DotNetEnv.Env.TraversePath().Load();
                string db_username = Environment.GetEnvironmentVariable("db_username");
                string db_password = Environment.GetEnvironmentVariable("db_password");
                string db_host = Environment.GetEnvironmentVariable("db_host");

                string dbcs = $"Server={db_host};Database=MoviesApp;Uid={db_username};Pwd={db_password};";
                options.UseMySql(
                    connectionString: dbcs,
                    new MySqlServerVersion(new Version(8, 0, 27))
                );
            });

            serviceProvider = services.BuildServiceProvider();
        }
    }
}