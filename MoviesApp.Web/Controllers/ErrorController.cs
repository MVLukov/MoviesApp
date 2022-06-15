using Microsoft.AspNetCore.Mvc;


namespace MoviesApp.Controllers;

public class ErrorController : Controller
{

    public IActionResult Error(int? code = null)
    {
        if (code != null && code == 404)
        {
            ViewData["Title"] = "NotFound";
            return View("NotFound");
        }

        return View("Error");
    }
}