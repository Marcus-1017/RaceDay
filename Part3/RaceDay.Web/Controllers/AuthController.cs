using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class AuthController : AppController
{
    public AuthController(ApiClient api) : base(api) { }

    [HttpGet]
    public IActionResult Register() => View(new RegisterRequest { Role = "Participant" });

    [HttpPost]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        if (!ModelState.IsValid) return View(model);

        var response = await Api.SendAsync(HttpMethod.Post, "api/auth/register", model);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ApiClient.ErrorAsync(response));
            return View(model);
        }

        TempData["Success"] = "Registration successful. Please log in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginRequest());

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        if (!ModelState.IsValid) return View(model);

        HttpResponseMessage response;
        try
        {
            response = await Api.SendAsync(HttpMethod.Post, "api/auth/login", model);
        }
        catch
        {
            ModelState.AddModelError("", "Cannot reach the RaceDay server. Please try again later.");
            return View(model);
        }

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        var auth = await Api.ReadAsync<AuthResponse>(response);
        var cookie = ApiClient.ExtractCookie(response);
        if (auth == null || cookie == null)
        {
            ModelState.AddModelError("", "Login failed. Please try again.");
            return View(model);
        }

        // MVC session stores who is logged in; the API cookie proves it to the API
        HttpContext.Session.SetString("ApiCookie", cookie);
        HttpContext.Session.SetInt32("UserId", auth.UserID);
        HttpContext.Session.SetString("Role", auth.Role);
        HttpContext.Session.SetString("Name", auth.FullName);

        return auth.Role == "Organiser"
            ? RedirectToAction("Index", "Organiser")
            : RedirectToAction("Index", "Participant");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        try { await Api.SendAsync(HttpMethod.Post, "api/auth/logout"); } catch { }
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}