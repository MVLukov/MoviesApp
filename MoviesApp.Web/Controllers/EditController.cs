using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoviesApp.Data;

namespace MoviesApp.Controllers;

public class EditController : Controller
{
    private readonly MoviesAppDbContext _context;   //db connection
    private List<SelectListItem> getGenres = Enum.GetValues(typeof(Genre.Genres)).Cast<Genre.Genres>().Select(x => new SelectListItem { Value = x.ToString(), Text = x.ToString() }).ToList();  //genre list
    private List<String> genresList = Enum.GetValues(typeof(Genre.Genres)).Cast<Genre.Genres>().Select(x => x.ToString()).ToList(); //list of genres raw
    private readonly ILogger _logger;

    public EditController(MoviesAppDbContext context, ILogger<EditController> logger)
    {
        _context = context;     //set db connection
        _logger = logger;   //set logger
    }

    [HttpGet]
    [ActionName("Movie")]
    public async Task<IActionResult> Movie_Get(int id)
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                ViewData["canConnect"] = false;
                return View("Movie");
            }

            //get movie with given id
            Movie getMovie = await _context.Movies.Include(x => x.Actors).Include(x => x.Director).FirstOrDefaultAsync(x => x.MovieId == id);

            //check if movie with given id exists
            if (getMovie != null)
            {
                //generate lists of directors/actors
                List<SelectListItem> getActors = await _context.Actors.Select(x => new SelectListItem { Value = x.ActorId.ToString(), Text = x.ActorName }).ToListAsync();
                List<SelectListItem> getDirectors = await _context.Directors.Select(x => new SelectListItem { Value = x.DirectorId.ToString(), Text = x.DirectorName }).ToListAsync();
                getDirectors.Add(new SelectListItem { Value = 0.ToString(), Text = "Not specified", Selected = true });

                //send actors/directors/genres lists to view
                ViewBag.actors = getActors;
                ViewBag.directors = getDirectors;
                ViewBag.genres = getGenres;

                return View("Movie", getMovie);
            }
            else
            {
                ViewData["notFound"] = true;
            }

            return View("Movie");
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
                return View("Movie");
            }

            //generate lists of actors/directors
            List<SelectListItem> getActors = await _context.Actors.Select(x => new SelectListItem { Value = x.ActorId.ToString(), Text = x.ActorName }).ToListAsync();
            List<SelectListItem> getDirectors = await _context.Directors.Select(x => new SelectListItem { Value = x.DirectorId.ToString(), Text = x.DirectorName }).ToListAsync();
            getDirectors.Add(new SelectListItem { Value = 0.ToString(), Text = "Not specified", Selected = true });

            //send actors/directors/genres lists to view
            ViewBag.actors = getActors;
            ViewBag.directors = getDirectors;
            ViewBag.genres = getGenres;

            //filter movie's name
            movie.MovieName = Filters.movieNameFilter.Replace(movie.MovieName, String.Empty);
            movie.MovieName = Filters.singleSpace.Replace(movie.MovieName, " ");
            movie.MovieName = movie.MovieName.Trim();

            //filter movie's synopsis
            movie.MovieSynopsis = Filters.synopsisFilter.Replace(movie.MovieSynopsis, String.Empty);
            movie.MovieSynopsis = Filters.synopsisFilter.Replace(movie.MovieSynopsis, " ");
            movie.MovieSynopsis = movie.MovieSynopsis.Trim();

            movie.MovieRating = float.Parse(Filters.rating.Replace(movie.MovieRating.ToString(), String.Empty));
            movie.MovieYear = Filters.year.Replace(movie.MovieYear, String.Empty);
            movie.MovieGenre = Filters.genre.Replace(movie.MovieGenre, String.Empty);

            //get movie with given name
            Movie getMovie = await _context.Movies.FirstOrDefaultAsync(x => x.MovieName.ToLower() == movie.MovieName.ToLower());

            //check if movie with given name exists
            if (getMovie != null && getMovie.MovieId != movie.MovieId)
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
                //get movie with given id
                getMovie = await _context.Movies.Include(x => x.Actors).Include(x => x.Director).FirstOrDefaultAsync(x => x.MovieId == movie.MovieId);

                //check if movie with given id exists
                if (getMovie != null)
                {
                    //update movie fields
                    getMovie.MovieName = movie.MovieName;
                    getMovie.MovieSynopsis = movie.MovieSynopsis;
                    getMovie.MovieYear = movie.MovieYear;
                    getMovie.MovieGenre = movie.MovieGenre;
                    getMovie.MovieRating = movie.MovieRating;

                    //get director with given id
                    Director getDirector = await _context.Directors.FirstOrDefaultAsync(x => x.DirectorId == movie.DirectorId);

                    //check if director with given id exists
                    if (getDirector != null)
                    {
                        getMovie.DirectorId = getDirector.DirectorId;   //set director id
                        getMovie.Director = getDirector;    //set director
                    }
                    else
                    {
                        //remove director
                        getMovie.DirectorId = null;
                        getMovie.Director = null;
                    }

                    List<Actor> actors = new List<Actor>();

                    foreach (var item in movie.ActorsSelect)
                    {
                        //get actor with given id
                        Actor getActor = await _context.Actors.FirstOrDefaultAsync(x => x.ActorId == item);

                        //check if actor with given id exists
                        if (getActor != null)
                        {
                            actors.Add(getActor);  //set actor
                        }
                    }

                    getMovie.Actors = actors;


                    ViewData["successful"] = true;
                    await _context.SaveChangesAsync();
                    return View("Movie", getMovie);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return View("Movie", movie);
            }
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
    }

    [HttpGet]
    [ActionName("Actor")]
    public async Task<IActionResult> Actor_Get(int id)
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                ViewData["canConnect"] = false;
                return View("Actor");
            }

            //get actor with given in
            Actor getActor = await _context.Actors.FirstOrDefaultAsync(x => x.ActorId == id);

            //check if actor with given id exists
            if (getActor != null)
            {
                return View("Actor", getActor);
            }
            else
            {
                ViewData["notFound"] = true;
            }

            return View("Actor");
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
                return View("Actor");
            }

            //filter actor's name
            actor.ActorName = Filters.nameFilter.Replace(actor.ActorName, String.Empty);
            actor.ActorName = Filters.singleSpace.Replace(actor.ActorName, " ");
            actor.ActorName = actor.ActorName.Trim();

            //get actor with given name
            Actor getActor = await _context.Actors.FirstOrDefaultAsync(x => x.ActorName.ToLower() == actor.ActorName.ToLower() && x.ActorId != actor.ActorId);

            //check if actor with given name exists
            if (getActor != null)
            {
                ModelState.AddModelError(nameof(Actor.ActorName), "This actor already exists!");
            }

            if (ModelState.IsValid)
            {
                //get actor by id
                getActor = await _context.Actors.FirstOrDefaultAsync(x => x.ActorId == actor.ActorId);

                //check if actor with given id exists
                if (getActor != null)
                {
                    getActor.ActorName = actor.ActorName;

                    ViewData["successful"] = true;
                    await _context.SaveChangesAsync();
                    return View("Actor", getActor);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return View("Actor", actor);
            }
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
    }

    [HttpGet]
    [ActionName("Director")]
    public async Task<IActionResult> Director_Get(int id)
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                ViewData["canConnect"] = false;
                return View("Director");
            }

            //get director with given id
            Director getDirector = await _context.Directors.FirstOrDefaultAsync(x => x.DirectorId == id);

            //check if director with given id exists
            if (getDirector != null)
            {
                return View("Director", getDirector);
            }
            else
            {
                ViewData["notFound"] = true;
            }

            return View("Director");
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
                return View("Actor");
            }

            //filter director's name
            director.DirectorName = Filters.nameFilter.Replace(director.DirectorName, String.Empty);
            director.DirectorName = Filters.singleSpace.Replace(director.DirectorName, " ");
            director.DirectorName = director.DirectorName.Trim();

            //get director by given name
            Director getDirector = await _context.Directors.FirstOrDefaultAsync(x => x.DirectorName.ToLower() == director.DirectorName.ToLower());

            //check if director with given name exists
            if (getDirector != null && getDirector.DirectorId != director.DirectorId)
            {
                ModelState.AddModelError(nameof(Director.DirectorName), "This director already exists!");   //set error
            }

            if (ModelState.IsValid)
            {
                //get director with given id
                getDirector = await _context.Directors.FirstOrDefaultAsync(x => x.DirectorId == director.DirectorId);

                //check if director with given id exists
                if (getDirector != null)
                {
                    getDirector.DirectorName = director.DirectorName;

                    ViewData["successful"] = true;
                    await _context.SaveChangesAsync();
                    return View("Director", getDirector);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return View("Director", director);
            }
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
    }
}
