using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApp.Data;

namespace MoviesApp.Controllers;

public class DeleteController : Controller
{
    private readonly MoviesAppDbContext _context;   //db connection
    private readonly ILogger _logger;

    public DeleteController(MoviesAppDbContext context, ILogger<DeleteController> logger)
    {
        _context = context;     //set db connection
        _logger = logger;   //set logger
    }

    [HttpGet]
    public async Task<IActionResult> Movie(int id)
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                ViewData["canConnect"] = false;
                return Redirect("/Home/Index");
            }

            //get movie with given id
            Movie getMovie = await _context.Movies.FirstOrDefaultAsync(x => x.MovieId == id);

            //check if movie with given id exists
            if (getMovie != null)
            {
                _context.Movies.Remove(getMovie);
                await _context.SaveChangesAsync();
            }

            return Redirect("/Home/Index");
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Actor(int id)
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                ViewData["canConnect"] = false;
                return Redirect("/Home/Actors");
            }

            //get actor with given id
            Actor getActor = await _context.Actors.FirstOrDefaultAsync(x => x.ActorId == id);

            //check if actor with given id exists
            if (getActor != null)
            {
                _context.Actors.Remove(getActor);
                await _context.SaveChangesAsync();
            }

            return Redirect("/Home/Actors");
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Director(int id)
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                ViewData["canConnect"] = false;
                return Redirect("/Home/Directors");
            }

            //get director with given id
            Director getDirector = await _context.Directors.FirstOrDefaultAsync(x => x.DirectorId == id);

            if (getDirector != null)
            {
                //get all movies with given director
                List<Movie> getMovies = await _context.Movies.Where(x => x.DirectorId == id).ToListAsync();

                foreach (var item in getMovies)
                {
                    item.DirectorId = null;     //remove director from movie
                }

                await _context.SaveChangesAsync();  //save updated movies

                _context.Directors.Remove(getDirector);
                await _context.SaveChangesAsync();
            }

            return Redirect("/Home/Directors");
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
    }
}
