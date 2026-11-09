using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class ParticipantController : AppController
{
    public ParticipantController(ApiClient api) : base(api) { }

    public async Task<IActionResult> Index(string? type, string? search)
    {
        var g = Guard("Participant"); if (g != null) return g;

        var r = await Api.SendAsync(HttpMethod.Get, "api/events");
        var events = await Api.ReadAsync<List<EventItem>>(r) ?? new();

        events = events.Where(e => e.EventDate >= DateTime.Today).OrderBy(e => e.EventDate).ToList();
        if (!string.IsNullOrWhiteSpace(type))
            events = events.Where(e => e.EventType == type).ToList();
        if (!string.IsNullOrWhiteSpace(search))
            events = events.Where(e =>
                e.EventName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        ViewBag.Type = type;
        ViewBag.Search = search;
        return View(events);
    }

    public async Task<IActionResult> Details(int id)
    {
        var g = Guard("Participant"); if (g != null) return g;

        var er = await Api.SendAsync(HttpMethod.Get, $"api/events/{id}");
        var ev = await Api.ReadAsync<EventItem>(er);
        if (ev == null) return RedirectToAction(nameof(Index));

        var cr = await Api.SendAsync(HttpMethod.Get, $"api/events/{id}/categories");
        var cats = await Api.ReadAsync<List<CategoryItem>>(cr) ?? new();

        return View(new DetailsVm { Event = ev, Categories = cats });
    }

    [HttpPost]
    public async Task<IActionResult> EnterEvent(int eventId, int categoryId)
    {
        var g = Guard("Participant"); if (g != null) return g;

        var r = await Api.SendAsync(HttpMethod.Post, "api/enrolments",
            new EnrolRequest { EventId = eventId, CategoryId = categoryId });

        if (!r.IsSuccessStatusCode)
        {
            TempData["Error"] = await ApiClient.ErrorAsync(r);
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        TempData["Success"] = "You have entered the event.";
        return RedirectToAction(nameof(MyEvents));
    }

    public async Task<IActionResult> MyEvents()
    {
        var g = Guard("Participant"); if (g != null) return g;
        var r = await Api.SendAsync(HttpMethod.Get, "api/enrolments/mine");
        return View(await Api.ReadAsync<List<MyEnrolmentItem>>(r) ?? new());
    }

    public async Task<IActionResult> MyResults()
    {
        var g = Guard("Participant"); if (g != null) return g;
        var r = await Api.SendAsync(HttpMethod.Get, "api/results/mine");
        return View(await Api.ReadAsync<List<ResultItem>>(r) ?? new());
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var g = Guard("Participant"); if (g != null) return g;
        var r = await Api.SendAsync(HttpMethod.Get, "api/profile");
        var p = await Api.ReadAsync<ProfileItem>(r);
        if (p == null) return RedirectToAction("Login", "Auth");
        return View(new UpdateProfileRequest { FullName = p.FullName, Email = p.Email });
    }

    [HttpPost]
    public async Task<IActionResult> Profile(UpdateProfileRequest model)
    {
        var g = Guard("Participant"); if (g != null) return g;
        if (!ModelState.IsValid) return View(model);

        var r = await Api.SendAsync(HttpMethod.Put, "api/profile", model);
        if (!r.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ApiClient.ErrorAsync(r));
            return View(model);
        }

        TempData["Success"] = "Profile updated.";
        return RedirectToAction(nameof(Profile));
    }
}