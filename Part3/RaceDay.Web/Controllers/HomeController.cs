using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var role = HttpContext.Session.GetString("Role");
        if (role == "Organiser") return RedirectToAction("Index", "Organiser");
        if (role == "Participant") return RedirectToAction("Index", "Participant");
        return View();
    }

    public IActionResult Error() => View();
}
