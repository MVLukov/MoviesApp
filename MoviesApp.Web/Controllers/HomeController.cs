using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MoviesApp.Data;
using MoviesApp.Models;

namespace MoviesApp.Controllers;

public class HomeController : Controller
{
    private readonly MoviesAppDbContext _context;   //db connection
    private List<SelectListItem> getGenres = Enum.GetValues(typeof(Genre.Genres)).Cast<Genre.Genres>().Select(x => new SelectListItem { Value = x.ToString(), Text = x.ToString(), Selected = false }).ToList();    //genre list
    private List<SelectListItem> orderByName = new List<SelectListItem>() { new SelectListItem { Value = "ASC", Text = "A-Z", Selected = false }, new SelectListItem { Value = "DESC", Text = "Z-A", Selected = false }, new SelectListItem { Value = "undefined", Text = "Not selected", Selected = true } };  //name filter list
    private List<SelectListItem> orderByRating = new List<SelectListItem>() { new SelectListItem { Value = "ASC", Text = "1-10", Selected = false }, new SelectListItem { Value = "DESC", Text = "10-1", Selected = false }, new SelectListItem { Value = "undefined", Text = "Not selected", Selected = true } };  //rating filter list
    private List<SelectListItem> orderByYear = new List<SelectListItem>() { new SelectListItem { Value = "ASC", Text = "1900-2099", Selected = false }, new SelectListItem { Value = "DESC", Text = "2099-1900", Selected = false }, new SelectListItem { Value = "undefined", Text = "Not selected", Selected = true } };  //year filter list
    private readonly ILogger _logger;

    public HomeController(MoviesAppDbContext context, ILogger<HomeController> logger)
    {
        _context = context;     //set db connection
        _logger = logger;    //set logger
    }

    [HttpGet]
    public async Task<IActionResult> Index(string search, string[] genres, int currentPage = 1, string orderByNameFilter = "undefined", string orderByRatingFilter = "undefined", string orderByYearFilter = "undefined")
    {
        try
        {
            //set filters lists to view
            ViewBag.genres = getGenres;
            ViewBag.orderByName = orderByName;
            ViewBag.orderByRating = orderByRating;
            ViewBag.orderByYear = orderByYear;

            List<MovieView> moviesViewList = new List<MovieView>();     //movie list

            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                ViewData["canConnect"] = false;
                return View();
            }

            if (currentPage < 1) currentPage = 1;

            if (!String.IsNullOrEmpty(search) || genres.Any() || orderByNameFilter != "undefined" || orderByRatingFilter != "undefined" || orderByYearFilter != "undefined")
            {
                // List<MovieView> getMovies = await this.MoviesFilter(search, genres, currentPage, orderByNameFilter, orderByRatingFilter, orderByYearFilter);

                IActionResult actionResult = await this.MoviesFilter(search, genres, currentPage, orderByNameFilter, orderByRatingFilter, orderByYearFilter);
                ViewData["currentPage"] = currentPage;

                // return View(getMovies);
                return actionResult;
            }
            else
            {
                ViewData["pageCount"] = (int)Math.Ceiling((double)_context.Movies.Count() / 9);
                ViewData["currentPage"] = currentPage;

                // get all movies ordered by genre
                List<Movie> getMovies = await _context.Movies.Include(x => x.Actors).Include(x => x.Director).OrderBy(x => x.MovieGenre).AsNoTracking().ToListAsync();
                getMovies = getMovies.Skip(9 * (currentPage - 1)).Take(9).ToList();

                foreach (var item in getMovies)
                {
                    //add movieView to movie list
                    moviesViewList.Add(new MovieView(item.MovieId, item.MovieName, item.MovieSynopsis, item.MovieGenre, item.MovieYear, item.MovieRating, item.Director, item.Actors.ToList()));
                }

                if (!moviesViewList.Any())
                {
                    ViewData["notFound"] = true;
                    return View();
                }

                //send movie list to view
                return View(moviesViewList);
            }
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Actors(string search, int currentPage = 1)
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                ViewData["canConnect"] = false;
                return View();
            }

            if (currentPage < 1) currentPage = 1;

            if (!String.IsNullOrEmpty(search))
            {
                search = Filters.movieNameFilter.Replace(search, String.Empty);
                search = Filters.singleSpace.Replace(search, " ");
                search = search.Trim();

                IActionResult result = await ActorsSearch(search, currentPage);
                return result;
            }
            else
            {
                //get all actors
                List<Actor> getActors = await _context.Actors.OrderBy(x => x.ActorId).Skip(18 * (currentPage - 1)).Take(18).AsNoTracking().ToListAsync();

                //check for existing records
                if (getActors.Count == 0)
                {
                    ViewData["notFound"] = true;
                    return View();
                }

                ViewData["pageCount"] = (int)Math.Ceiling((double)_context.Actors.Count() / 18);
                ViewData["currentPage"] = currentPage;

                //send actors list to view
                return View(getActors.ToList());
            }
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Directors(string search, int currentPage = 1)
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                ViewData["canConnect"] = false;
                return View();
            }

            if (currentPage < 1) currentPage = 1;

            if (!String.IsNullOrEmpty(search))
            {
                search = Filters.movieNameFilter.Replace(search, String.Empty);
                search = Filters.singleSpace.Replace(search, " ");
                search = search.Trim();

                IActionResult result = await DirectorsSearch(search, currentPage);
                return result;
            }
            else
            {
                //get all directors
                List<Director> getDirectors = await _context.Directors.OrderBy(x => x.DirectorId).Skip(18 * (currentPage - 1)).Take(18).AsNoTracking().ToListAsync();

                ViewData["pageCount"] = (int)Math.Ceiling((double)_context.Directors.Count() / 18);
                ViewData["currentPage"] = currentPage;

                //check for existing records
                if (getDirectors.Count == 0)
                {
                    ViewData["notFound"] = true;
                    return View();
                }

                //send directors list to view
                return View(getDirectors);
            }
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
    }

    [NonAction]
    public async Task<IActionResult> DirectorsSearch(string search, int currentPage = 1)
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                ViewData["canConnect"] = false;
                return View("Directors");
            }

            if (!String.IsNullOrEmpty(search))
            {

                int getDirectorsCount = _context.Directors.Where(x => x.DirectorName.Contains(search)).Count();
                List<Director> getDirectors = _context.Directors.Where(x => x.DirectorName.Contains(search)).OrderBy(x => x.DirectorId).AsNoTracking().ToList();

                if (getDirectors.Count > 0)
                {
                    ViewData["pageCount"] = (int)Math.Ceiling((double)getDirectorsCount / 18);
                    ViewData["currentPage"] = currentPage;
                    return View("Directors", getDirectors.Skip(18 * (currentPage - 1)).Take(18).ToList());
                }
                else
                {
                    ViewData["notFound"] = true;
                    return View("Directors");
                }
            }
            else
            {
                return Redirect("/Home/Directors");
            }
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
    }

    [NonAction]
    public async Task<IActionResult> ActorsSearch(string search, int currentPage = 1)
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                ViewData["canConnect"] = false;
                return View("Actors");
            }

            if (!String.IsNullOrEmpty(search))
            {

                int getActorsCount = _context.Actors.Where(x => x.ActorName.Contains(search)).Count();
                List<Actor> getActors = _context.Actors.Where(x => x.ActorName.Contains(search)).OrderBy(x => x.ActorId).AsNoTracking().ToList();

                if (getActors.Count > 0)
                {
                    ViewData["pageCount"] = (int)Math.Ceiling((double)getActorsCount / 18);
                    ViewData["currentPage"] = currentPage;
                    return View("Actors", getActors.Skip(18 * (currentPage - 1)).Take(18).ToList());
                }
                else
                {
                    ViewData["notFound"] = true;
                    return View("Actors");
                }
            }
            else
            {
                return Redirect("/Home/Actors");
            }
        }
        catch (System.Exception e)
        {
            _logger.LogError(e.ToString());
            return View("Error");
        }
    }

    [NonAction]
    public async Task<IActionResult> MoviesFilter(string search, string[] genres, int currentPage, string orderByNameFilter, string orderByRatingFilter, string orderByYearFilter)
    {
        try
        {
            int pageCount = 0;
            //list of filtered movies
            List<Movie> filteredMovies = new List<Movie>();

            if (!String.IsNullOrEmpty(search))
            {
                search = Filters.movieNameFilter.Replace(search, String.Empty);
                search = Filters.singleSpace.Replace(search, " ");
                search = search.Trim();
            }

            //if year filter is applied set name/rating filter to null
            if (orderByYearFilter != "undefined")
            {
                orderByNameFilter = "undefined";
                orderByRatingFilter = "undefined";
            }

            //if rating filter is applied set name filter to null
            if (orderByRatingFilter != "undefined")
            {
                orderByNameFilter = "undefined";
            }

            if (genres.Length == 0 && orderByNameFilter == "undefined" && orderByRatingFilter == "undefined" && orderByYearFilter == "undefined" && !String.IsNullOrEmpty(search))
            {
                pageCount = _context.Movies.Where(x => x.MovieName.Contains(search)).Count();
                List<Movie> getMovies = await _context.Movies.Where(x => x.MovieName.Contains(search)).Include(x => x.Actors).Include(x => x.Director).ToListAsync();
                filteredMovies = getMovies.Skip(9 * (currentPage - 1)).Take(9).ToList();
            }
            else if (orderByNameFilter != "undefined")
            {
                if (!genres.Any() && String.IsNullOrEmpty(search))  //if no genres and empty search
                {
                    if (orderByNameFilter == "ASC")
                    {
                        filterMovieName(orderByNameFilter);
                        pageCount = _context.Movies.Count();
                        List<Movie> getMovies = await _context.Movies.OrderBy(x => x.MovieName).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies = getMovies.Skip(9 * (currentPage - 1)).Take(9).ToList();
                    }

                    if (orderByNameFilter == "DESC")
                    {
                        filterMovieName(orderByNameFilter);
                        pageCount = _context.Movies.Count();
                        List<Movie> getMovies = await _context.Movies.OrderByDescending(x => x.MovieName).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies = getMovies.Skip(9 * (currentPage - 1)).Take(9).ToList();
                    }
                }

                if (!genres.Any() && !String.IsNullOrEmpty(search)) //if no genres and search
                {
                    if (orderByNameFilter == "ASC")
                    {
                        filterMovieName(orderByNameFilter);
                        pageCount = _context.Movies.Where(x => x.MovieName.Contains(search)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => x.MovieName.Contains(search)).OrderBy(x => x.MovieName).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies = getMovies.Skip(9 * (currentPage - 1)).Take(9).ToList();
                    }

                    if (orderByNameFilter == "DESC")
                    {
                        filterMovieName(orderByNameFilter);
                        pageCount = _context.Movies.Where(x => x.MovieName.Contains(search)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => x.MovieName.Contains(search)).OrderByDescending(x => x.MovieName).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies = getMovies.Skip(9 * (currentPage - 1)).Take(9).ToList();
                    }
                }

                if (genres.Any() && String.IsNullOrEmpty(search))   //if genres and search
                {
                    if (orderByNameFilter == "ASC")
                    {
                        foreach (var item in genres)
                        {
                            getGenres.Single(x => x.Value == item).Selected = true;
                        }

                        filterMovieName(orderByNameFilter);
                        pageCount += _context.Movies.Where(x => genres.Contains(x.MovieGenre)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => genres.Contains(x.MovieGenre)).OrderBy(x => x.MovieName).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies.AddRange(getMovies.Skip(9 * (currentPage - 1)).Take(9));

                    }

                    if (orderByNameFilter == "DESC")
                    {
                        foreach (var item in genres)
                        {
                            getGenres.Single(x => x.Value == item).Selected = true;
                        }
                        filterMovieName(orderByNameFilter);
                        pageCount += _context.Movies.Where(x => genres.Contains(x.MovieGenre)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => genres.Contains(x.MovieGenre)).OrderByDescending(x => x.MovieName).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies.AddRange(getMovies.Skip(9 * (currentPage - 1)).Take(9));

                    }
                }

                if (genres.Any() && !String.IsNullOrEmpty(search))  //if genres and empty search
                {
                    if (orderByNameFilter == "ASC")
                    {
                        foreach (var item in genres)
                        {
                            getGenres.Single(x => x.Value == item).Selected = true;
                        }

                        filterMovieName(orderByNameFilter);
                        pageCount += _context.Movies.Where(x => genres.Contains(x.MovieGenre) && x.MovieName.Contains(search)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => genres.Contains(x.MovieGenre) && x.MovieName.Contains(search)).OrderBy(x => x.MovieName).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies.AddRange(getMovies.Skip(9 * (currentPage - 1)).Take(9));

                    }

                    if (orderByNameFilter == "DESC")
                    {
                        foreach (var item in genres)
                        {
                            getGenres.Single(x => x.Value == item).Selected = true;
                        }
                        filterMovieName(orderByNameFilter);
                        pageCount += _context.Movies.Where(x => genres.Contains(x.MovieGenre) && x.MovieName.Contains(search)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => genres.Contains(x.MovieGenre) && x.MovieName.Contains(search)).OrderByDescending(x => x.MovieName).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies.AddRange(getMovies.Skip(9 * (currentPage - 1)).Take(9));

                    }
                }
            }
            else if (orderByRatingFilter != "undefined")
            {
                if (!genres.Any() && String.IsNullOrEmpty(search))  //if no genres and empty search
                {
                    if (orderByRatingFilter == "ASC")
                    {
                        filterMovieRating(orderByRatingFilter);
                        pageCount = _context.Movies.Count();
                        List<Movie> getMovies = await _context.Movies.OrderBy(x => x.MovieRating).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies = getMovies.Skip(9 * (currentPage - 1)).Take(9).ToList();
                    }

                    if (orderByRatingFilter == "DESC")
                    {
                        filterMovieRating(orderByRatingFilter);
                        pageCount = _context.Movies.Count();
                        List<Movie> getMovies = await _context.Movies.OrderByDescending(x => x.MovieRating).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies = getMovies.Skip(9 * (currentPage - 1)).Take(9).ToList();
                    }
                }

                if (!genres.Any() && !String.IsNullOrEmpty(search)) //if no genres and search
                {
                    if (orderByRatingFilter == "ASC")
                    {
                        filterMovieRating(orderByRatingFilter);
                        pageCount = _context.Movies.Where(x => x.MovieName.Contains(search)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => x.MovieName.Contains(search)).OrderBy(x => x.MovieRating).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies = getMovies.Skip(9 * (currentPage - 1)).Take(9).ToList();
                    }

                    if (orderByRatingFilter == "DESC")
                    {
                        filterMovieRating(orderByRatingFilter);
                        pageCount = _context.Movies.Where(x => x.MovieName.Contains(search)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => x.MovieName.Contains(search)).OrderByDescending(x => x.MovieRating).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies = getMovies.Skip(9 * (currentPage - 1)).Take(9).ToList();
                    }
                }

                if (genres.Any() && String.IsNullOrEmpty(search))   //if genres and no search
                {
                    if (orderByRatingFilter == "ASC")
                    {
                        foreach (var item in genres)
                        {
                            getGenres.Single(x => x.Value == item).Selected = true;
                        }

                        filterMovieRating(orderByRatingFilter);
                        pageCount += _context.Movies.Where(x => genres.Contains(x.MovieGenre)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => genres.Contains(x.MovieGenre)).OrderBy(x => x.MovieRating).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies.AddRange(getMovies.Skip(9 * (currentPage - 1)).Take(9));

                    }

                    if (orderByRatingFilter == "DESC")
                    {
                        foreach (var item in genres)
                        {
                            getGenres.Single(x => x.Value == item).Selected = true;
                        }

                        filterMovieRating(orderByRatingFilter);
                        pageCount += _context.Movies.Where(x => genres.Contains(x.MovieGenre)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => genres.Contains(x.MovieGenre)).OrderByDescending(x => x.MovieRating).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies.AddRange(getMovies.Skip(9 * (currentPage - 1)).Take(9));
                    }
                }

                if (genres.Any() && !String.IsNullOrEmpty(search))  //if genres and search
                {
                    if (orderByRatingFilter == "ASC")
                    {
                        foreach (var item in genres)
                        {
                            getGenres.Single(x => x.Value == item).Selected = true;
                        }

                        filterMovieRating(orderByRatingFilter);
                        pageCount += _context.Movies.Where(x => genres.Contains(x.MovieGenre) && x.MovieName.Contains(search)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => genres.Contains(x.MovieGenre) && x.MovieName.Contains(search)).OrderBy(x => x.MovieRating).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies.AddRange(getMovies.Skip(9 * (currentPage - 1)).Take(9));

                    }

                    if (orderByRatingFilter == "DESC")
                    {
                        foreach (var item in genres)
                        {
                            getGenres.Single(x => x.Value == item).Selected = true;
                        }

                        filterMovieRating(orderByRatingFilter);
                        pageCount += _context.Movies.Where(x => genres.Contains(x.MovieGenre) && x.MovieName.Contains(search)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => genres.Contains(x.MovieGenre) && x.MovieName.Contains(search)).OrderByDescending(x => x.MovieRating).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies.AddRange(getMovies.Skip(9 * (currentPage - 1)).Take(9));
                    }
                }
            }
            else if (orderByYearFilter != "undefined")
            {
                if (!genres.Any() && String.IsNullOrEmpty(search))  //if no genres and empty search
                {
                    if (orderByYearFilter == "ASC")
                    {
                        filterMovieYear(orderByYearFilter);
                        pageCount = _context.Movies.Count();
                        List<Movie> getMovies = await _context.Movies.OrderBy(x => x.MovieYear).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies = getMovies.Skip(9 * (currentPage - 1)).Take(9).ToList();
                    }

                    if (orderByYearFilter == "DESC")
                    {
                        filterMovieYear(orderByYearFilter);
                        pageCount = _context.Movies.Count();
                        List<Movie> getMovies = await _context.Movies.OrderByDescending(x => x.MovieYear).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies = getMovies.Skip(9 * (currentPage - 1)).Take(9).ToList();
                    }
                }

                if (!genres.Any() && !String.IsNullOrEmpty(search)) //if no genres and search
                {
                    if (orderByYearFilter == "ASC")
                    {
                        filterMovieYear(orderByYearFilter);
                        pageCount = _context.Movies.Where(x => x.MovieName.Contains(search)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => x.MovieName.Contains(search)).OrderBy(x => x.MovieYear).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies = getMovies.Skip(9 * (currentPage - 1)).Take(9).ToList();
                    }

                    if (orderByYearFilter == "DESC")
                    {
                        filterMovieYear(orderByYearFilter);
                        pageCount = _context.Movies.Where(x => x.MovieName.Contains(search)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => x.MovieName.Contains(search)).OrderByDescending(x => x.MovieYear).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies = getMovies.Skip(9 * (currentPage - 1)).Take(9).ToList();
                    }
                }

                if (genres.Any() && String.IsNullOrEmpty(search))   //if genres and no search
                {
                    if (orderByYearFilter == "ASC")
                    {
                        foreach (var item in genres)
                        {
                            getGenres.Single(x => x.Value == item).Selected = true;
                        }

                        filterMovieYear(orderByYearFilter);
                        pageCount += _context.Movies.Where(x => genres.Contains(x.MovieGenre)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => genres.Contains(x.MovieGenre)).OrderBy(x => x.MovieYear).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies.AddRange(getMovies.Skip(9 * (currentPage - 1)).Take(9));

                    }

                    if (orderByYearFilter == "DESC")
                    {
                        foreach (var item in genres)
                        {
                            getGenres.Single(x => x.Value == item).Selected = true;
                        }

                        filterMovieYear(orderByYearFilter);
                        pageCount += _context.Movies.Where(x => genres.Contains(x.MovieGenre)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => genres.Contains(x.MovieGenre)).OrderByDescending(x => x.MovieYear).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies.AddRange(getMovies.Skip(9 * (currentPage - 1)).Take(9));
                    }
                }

                if (genres.Any() && !String.IsNullOrEmpty(search))  //if genres and search
                {
                    if (orderByYearFilter == "ASC")
                    {
                        foreach (var item in genres)
                        {
                            getGenres.Single(x => x.Value == item).Selected = true;
                        }

                        filterMovieYear(orderByYearFilter);
                        pageCount += _context.Movies.Where(x => genres.Contains(x.MovieGenre) && x.MovieName.Contains(search)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => genres.Contains(x.MovieGenre) && x.MovieName.Contains(search)).OrderBy(x => x.MovieYear).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies.AddRange(getMovies.Skip(9 * (currentPage - 1)).Take(9));

                    }

                    if (orderByYearFilter == "DESC")
                    {
                        foreach (var item in genres)
                        {
                            getGenres.Single(x => x.Value == item).Selected = true;
                        }

                        filterMovieYear(orderByYearFilter);
                        pageCount += _context.Movies.Where(x => genres.Contains(x.MovieGenre) && x.MovieName.Contains(search)).Count();
                        List<Movie> getMovies = await _context.Movies.Where(x => genres.Contains(x.MovieGenre) && x.MovieName.Contains(search)).OrderByDescending(x => x.MovieYear).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                        filteredMovies.AddRange(getMovies.Skip(9 * (currentPage - 1)).Take(9));
                    }
                }
            }
            else if (genres.Length > 0)
            {
                if (genres.Any() && !String.IsNullOrEmpty(search))  //if genres and search
                {
                    foreach (var item in genres)
                    {
                        getGenres.Single(x => x.Value == item).Selected = true;
                    }

                    pageCount += _context.Movies.Where(x => genres.Contains(x.MovieGenre) && x.MovieName.Contains(search)).Count();
                    List<Movie> getMovies = await _context.Movies.Where(x => genres.Contains(x.MovieGenre) && x.MovieName.Contains(search)).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                    filteredMovies.AddRange(getMovies.Skip(9 * (currentPage - 1)).Take(9));
                }

                if (genres.Any() && String.IsNullOrEmpty(search))   //if genres and no search
                {
                    foreach (var item in genres)
                    {
                        getGenres.Single(x => x.Value == item).Selected = true;
                    }

                    pageCount += _context.Movies.Where(x => genres.Contains(x.MovieGenre)).Count();
                    List<Movie> getMovies = await _context.Movies.Where(x => genres.Contains(x.MovieGenre)).Include(x => x.Actors).Include(x => x.Director).AsNoTracking().ToListAsync();
                    filteredMovies.AddRange(getMovies.Skip(9 * (currentPage - 1)).Take(9));
                }
            }

            //movieView list
            List<MovieView> movies = new List<MovieView>();
            foreach (var item in filteredMovies)
            {
                movies.Add(new MovieView(item.MovieId, item.MovieName, item.MovieSynopsis, item.MovieGenre, item.MovieYear, item.MovieRating, item.Director, item.Actors.ToList()));
            }

            //set filters lists to view
            ViewBag.orderByName = orderByName;
            ViewBag.genres = getGenres;
            ViewBag.orderByRating = orderByRating;
            ViewBag.orderByYear = orderByYear;

            if (filteredMovies.Count == 0)
            {
                ViewData["notFound"] = true;
                return View("Index");
            }

            ViewData["pageCount"] = (int)Math.Ceiling((double)pageCount / 9);
            ViewData["currentPage"] = currentPage;

            return View("Index", movies);
        }
        catch (System.Exception)
        {

            throw;
        }
    }

    private void filterMovieName(string filter)
    {
        orderByRating.ForEach(x => x.Selected = false);
        orderByRating.Single(x => x.Value == "undefined").Selected = true;

        orderByYear.ForEach(x => x.Selected = false);
        orderByYear.Single(x => x.Value == "undefined").Selected = true;

        orderByName.Single(x => x.Value == filter).Selected = true;
        orderByName.Single(x => x.Value == "undefined").Selected = false;
    }

    private void filterMovieRating(string filter)
    {
        orderByName.ForEach(x => x.Selected = false);
        orderByName.Single(x => x.Value == "undefined").Selected = true;

        orderByYear.ForEach(x => x.Selected = false);
        orderByYear.Single(x => x.Value == "undefined").Selected = true;

        orderByRating.Single(x => x.Value == filter).Selected = true;
        orderByRating.Single(x => x.Value == "undefined").Selected = false;
    }

    private void filterMovieYear(string filter)
    {
        orderByName.ForEach(x => x.Selected = false);
        orderByName.Single(x => x.Value == "undefined").Selected = true;

        orderByRating.ForEach(x => x.Selected = false);
        orderByRating.Single(x => x.Value == "undefined").Selected = true;

        orderByYear.Single(x => x.Value == filter).Selected = true;
        orderByYear.Single(x => x.Value == "undefined").Selected = false;
    }
}
