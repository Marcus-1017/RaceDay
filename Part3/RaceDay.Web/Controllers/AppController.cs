using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public abstract class AppController : Controller
{
    protected readonly ApiClient Api;

    protected AppController(ApiClient api)
    {
        Api = api;
    }

    protected int? UserId => HttpContext.Session.GetInt32("UserId");
    protected string? Role => HttpContext.Session.GetString("Role");

    // returns null if allowed, otherwise a redirect
    protected IActionResult? Guard(string role)
    {
        if (UserId == null) return RedirectToAction("Login", "Auth");
        if (Role != role) return RedirectToAction("Index", "Home");
        return null;
    }
}