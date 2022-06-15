using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MoviesApp.Data;
using Microsoft.EntityFrameworkCore;

namespace MoviesApp.Controllers;
public class CreateController : Controller
{
    private readonly MoviesAppDbContext _context;   //db connection
    private List<SelectListItem> getGenres = Enum.GetValues(typeof(Genre.Genres)).Cast<Genre.Genres>().Select(x => new SelectListItem { Value = x.ToString(), Text = x.ToString() }).ToList();  //genre list 
    private List<String> genresList = Enum.GetValues(typeof(Genre.Genres)).Cast<Genre.Genres>().Select(x => x.ToString()).ToList(); //list of genres raw
    private readonly ILogger _logger;

    public CreateController(MoviesAppDbContext context, ILogger<CreateController> logger)
    {
        _context = context;     //set db connection
        _logger = logger;   //set logger
    }

    [HttpGet]
    [ActionName("Actor")]
    public async Task<IActionResult> Actor_Get()
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                ViewData["canConnect"] = false;
                return View();
            }

            return View();
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
    }

    [HttpPost]
    [ActionName("Actor")]
    public async Task<IActionResult> Actor_Post(Actor actor)
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                ViewData["canConnect"] = false;
                return View();
            }

            //filter actor's name
            actor.ActorName = Filters.nameFilter.Replace(actor.ActorName, String.Empty);
            actor.ActorName = Filters.singleSpace.Replace(actor.ActorName, " ");
            actor.ActorName = actor.ActorName.Trim();

            //get actor with given name
            Actor getActor = await _context.Actors.FirstOrDefaultAsync(x => x.ActorName.ToLower() == actor.ActorName.ToLower());

            //if actor with given name exists
            if (getActor != null)
            {
                ModelState.AddModelError(nameof(Actor.ActorName), "This actor already exists!");    //set error
            }

            if (ModelState.IsValid)
            {
                ViewData["successful"] = true;
                _context.Actors.Add(actor);
                await _context.SaveChangesAsync();
            }
            else
            {
                return View(actor);
            }

            return View();
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
    }

    [HttpGet]
    [ActionName("Director")]
    public async Task<IActionResult> Director_Get()
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                ViewData["canConnect"] = false;
                return View();
            }

            return View();
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
    }

    [HttpPost]
    [ActionName("Director")]
    public async Task<IActionResult> Director_Post(Director director)
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                ViewData["canConnect"] = false;
                return View();
            }

            //filter director name
            director.DirectorName = Filters.nameFilter.Replace(director.DirectorName, String.Empty);
            director.DirectorName = Filters.singleSpace.Replace(director.DirectorName, " ");
            director.DirectorName = director.DirectorName.Trim();

            //get director with given name 
            Director getDirector = await _context.Directors.FirstOrDefaultAsync(x => x.DirectorName.ToLower() == director.DirectorName.ToLower());

            //if director with given exists
            if (getDirector != null)
            {
                ModelState.AddModelError(nameof(Director.DirectorName), "This director already exists!");   //set error
            }

            if (ModelState.IsValid)
            {
                ViewData["successful"] = true;
                _context.Directors.Add(director);
                await _context.SaveChangesAsync();
            }
            else
            {
                return View(director);
            }

            return View();
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }

    }

    [HttpGet]
    [ActionName("Movie")]
    public async Task<IActionResult> Movie_Get()
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                ViewData["canConnect"] = false;
                return View();
            }

            List<SelectListItem> getActors = await _context.Actors.Select(x => new SelectListItem { Value = x.ActorId.ToString(), Text = x.ActorName, Selected = false }).ToListAsync();
            List<SelectListItem> getDirectors = await _context.Directors.Select(x => new SelectListItem { Value = x.DirectorId.ToString(), Text = x.DirectorName, Selected = false }).ToListAsync();
            getDirectors.Add(new SelectListItem { Value = 0.ToString(), Text = "Not specified", Selected = true });

            //send lists of actors/directors/genres to view
            ViewBag.actors = getActors;
            ViewBag.directors = getDirectors;
            ViewBag.genres = getGenres;

            return View();
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
    }

    [HttpPost]
    [ActionName("Movie")]
    public async Task<IActionResult> Movie_Post(Movie movie)
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                ViewData["canConnect"] = false;
                return View();
            }

            List<SelectListItem> getActors = await _context.Actors.Select(x => new SelectListItem { Value = x.ActorId.ToString(), Text = x.ActorName }).ToListAsync();
            List<SelectListItem> getDirectors = await _context.Directors.Select(x => new SelectListItem { Value = x.DirectorId.ToString(), Text = x.DirectorName }).ToListAsync();
            getDirectors.Add(new SelectListItem { Value = 0.ToString(), Text = "Not specified", Selected = true });

            //send lists of actors/directors/genres to view
            ViewBag.actors = getActors;
            ViewBag.directors = getDirectors;
            ViewBag.genres = getGenres;

            //filter movie's name
            movie.MovieName = Filters.movieNameFilter.Replace(movie.MovieName, String.Empty);
            movie.MovieName = Filters.singleSpace.Replace(movie.MovieName, " ");
            movie.MovieName = movie.MovieName.Trim();

            //filter movie's synopsis
            movie.MovieSynopsis = Filters.synopsisFilter.Replace(movie.MovieSynopsis, String.Empty);
            movie.MovieSynopsis = Filters.singleSpace.Replace(movie.MovieSynopsis, " ");
            movie.MovieSynopsis = movie.MovieSynopsis.Trim();

            movie.MovieRating = float.Parse(Filters.rating.Replace(movie.MovieRating.ToString(), String.Empty));
            movie.MovieYear = Filters.year.Replace(movie.MovieYear, String.Empty);
            movie.MovieGenre = Filters.genre.Replace(movie.MovieGenre, String.Empty);

            //get movie with given name
            Movie getMovie = await _context.Movies.FirstOrDefaultAsync(x => x.MovieName.ToLower() == movie.MovieName.ToLower());

            //check if movie with given name exists
            if (getMovie != null)
            {
                ModelState.AddModelError(nameof(Movie.MovieName), "This movie already exists!");    //set error
            }

            int checkGenre = genresList.FindIndex(x => x.ToLower() == movie.MovieGenre);

            if (checkGenre == -1)
            {
                movie.MovieGenre = "Unknown";
            }

            if (ModelState.IsValid)
            {
                //get director with given id
                Director getDirector = await _context.Directors.FirstOrDefaultAsync(x => x.DirectorId == movie.DirectorId);

                if (getDirector != null)
                {
                    movie.Director = getDirector;   //set director
                    movie.DirectorId = getDirector.DirectorId; //set director id
                }
                else
                {
                    movie.Director = null;
                    movie.DirectorId = null;
                }

                foreach (var item in movie.ActorsSelect)
                {
                    //get actor with given id
                    Actor getActor = await _context.Actors.FirstOrDefaultAsync(x => x.ActorId == item);

                    if (getActor != null)
                    {
                        movie.Actors.Add(getActor);     //add actor to actors list
                    }
                }

                ViewData["successful"] = true;
                _context.Movies.Add(movie);
                await _context.SaveChangesAsync();
            }
            else
            {
                return View(movie);
            }

            return View();
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
    }
}