using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApp.Data;
using MoviesApp.Models;

namespace MoviesApp.Controllers;
public class DetailsController : Controller
{
    private readonly MoviesAppDbContext _context;   //db connection
    private readonly ILogger _logger;

    public DetailsController(MoviesAppDbContext context, ILogger<DetailsController> logger)
    {
        _context = context;     //set db connection
        _logger = logger;   //set logger
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
                return View();
            }

            //get actor with given id
            Actor getActor = await _context.Actors.Include(x => x.Movies).FirstOrDefaultAsync(x => x.ActorId == id);

            //check if actor with given id exists
            if (getActor != null)
            {
                return View(new ActorView(getActor.ActorId, getActor.ActorName, getActor.Movies.ToList()));
            }
            else
            {
                ViewData["notFound"] = true;
                return View();
            }
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
                return View();
            }

            //get director with given id
            Director getDirector = await _context.Directors.FirstOrDefaultAsync(x => x.DirectorId == id);

            //check if director with given id exists
            if (getDirector != null)
            {
                //get list of movies with given director
                List<Movie> getMovies = await _context.Movies.Where(x => x.DirectorId == id).ToListAsync();

                //return directorView with view
                return View(new DirectorView(getDirector.DirectorId, getDirector.DirectorName, getMovies));
            }
            else
            {
                ViewData["notFound"] = true;
                return View();
            }
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
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
                return View();
            }

            //get movie with given id
            Movie getMovie = await _context.Movies.Include(x => x.Actors).Include(x => x.Director).FirstOrDefaultAsync(x => x.MovieId == id);

            //check if movie with given id exists
            if (getMovie != null)
            {
                //return movieView with view
                return View(new MovieView(getMovie.MovieId, getMovie.MovieName, getMovie.MovieSynopsis, getMovie.MovieGenre, getMovie.MovieYear, getMovie.MovieRating, getMovie.Director, getMovie.Actors.ToList()));
            }
            else
            {
                ViewData["notFound"] = true;
                return View();
            }
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
    }
}