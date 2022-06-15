using MoviesApp.Data;
using Microsoft.EntityFrameworkCore;
using MoviesApp.DataSeeder;

namespace MoviesApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DotNetEnv.Env.TraversePath().Load();
            var builder = WebApplication.CreateBuilder(args);

            //logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddTransient<DataSeederRawFile>();
            builder.Services.AddHealthChecks();

            builder.Services.AddDbContext<MoviesAppDbContext>(options =>
            {
                string db_username = Environment.GetEnvironmentVariable("db_username");
                string db_password = Environment.GetEnvironmentVariable("db_password");
                string db_host = Environment.GetEnvironmentVariable("db_host");
                string db_name = Environment.GetEnvironmentVariable("db_name");

                string dbcs = $"Server={db_host};Database={db_name};Uid={db_username};Pwd={db_password};";
                options.UseMySql(
                    connectionString: dbcs,
                    new MySqlServerVersion(new Version(8, 0, 27))
                );
            });

            var app = builder.Build();

            //load data to db
            SeedData(app);

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseStatusCodePagesWithRedirects("/Error/Error?code={0}");
            app.UseRouting();

            app.MapHealthChecks("/health");

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        private static void SeedData(IHost app)
        {
            var scopedFactory = app.Services.GetService<IServiceScopeFactory>();

            string db_seed = Environment.GetEnvironmentVariable("db_seed");

            using (var scope = scopedFactory.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<DataSeederRawFile>();
                if (db_seed == "true")
                {
                    var result = service.ReadRawFile();
                    service.Seed(result);
                }
                else
                {
                    service.createDb();
                }
            }
        }
    }
}