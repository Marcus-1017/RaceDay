using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class OrganiserController : AppController
{
    public OrganiserController(ApiClient api) : base(api) { }

    private async Task<List<EventItem>> MyEvents()
    {
        var r = await Api.SendAsync(HttpMethod.Get, "api/events");
        var all = await Api.ReadAsync<List<EventItem>>(r) ?? new();
        return all.Where(e => e.OrganiserID == UserId).OrderBy(e => e.EventDate).ToList();
    }

    public async Task<IActionResult> Index()
    {
        var g = Guard("Organiser"); if (g != null) return g;

        var rows = new List<DashboardRow>();
        foreach (var e in await MyEvents())
        {
            var r = await Api.SendAsync(HttpMethod.Get, $"api/events/{e.EventID}/enrolments");
            var list = await Api.ReadAsync<List<EnrolmentItem>>(r) ?? new();
            rows.Add(new DashboardRow { Event = e, Enrolments = list.Count });
        }
        return View(rows);
    }

    public async Task<IActionResult> Events()
    {
        var g = Guard("Organiser"); if (g != null) return g;
        return View(await MyEvents());
    }

    [HttpGet]
    public async Task<IActionResult> EventForm(int? id)
    {
        var g = Guard("Organiser"); if (g != null) return g;

        if (id == null)
            return View(new EventFormVm { EventDate = DateTime.Today.AddDays(30), EventType = "Run" });

        var r = await Api.SendAsync(HttpMethod.Get, $"api/events/{id}");
        var e = await Api.ReadAsync<EventItem>(r);
        if (e == null || e.OrganiserID != UserId) return RedirectToAction(nameof(Events));

        return View(new EventFormVm
        {
            EventID = e.EventID,
            EventName = e.EventName,
            Description = e.Description,
            EventDate = e.EventDate,
            Location = e.Location,
            Distance = e.Distance,
            EventType = e.EventType
        });
    }

    [HttpPost]
    public async Task<IActionResult> EventForm(EventFormVm model)
    {
        var g = Guard("Organiser"); if (g != null) return g;
        if (!ModelState.IsValid) return View(model);

        var response = model.EventID == 0
            ? await Api.SendAsync(HttpMethod.Post, "api/events", model)
            : await Api.SendAsync(HttpMethod.Put, $"api/events/{model.EventID}", model);

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", await ApiClient.ErrorAsync(response));
            return View(model);
        }

        TempData["Success"] = "Event saved.";
        return RedirectToAction(nameof(Events));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var g = Guard("Organiser"); if (g != null) return g;

        var r = await Api.SendAsync(HttpMethod.Delete, $"api/events/{id}");
        if (r.IsSuccessStatusCode) TempData["Success"] = "Event deleted.";
        else TempData["Error"] = await ApiClient.ErrorAsync(r);
        return RedirectToAction(nameof(Events));
    }

    // one page for categories, enrolments and results of a single event
    public async Task<IActionResult> Manage(int id)
    {
        var g = Guard("Organiser"); if (g != null) return g;

        var er = await Api.SendAsync(HttpMethod.Get, $"api/events/{id}");
        var ev = await Api.ReadAsync<EventItem>(er);
        if (ev == null || ev.OrganiserID != UserId) return RedirectToAction(nameof(Events));

        var cr = await Api.SendAsync(HttpMethod.Get, $"api/events/{id}/categories");
        var enr = await Api.SendAsync(HttpMethod.Get, $"api/events/{id}/enrolments");

        return View(new ManageVm
        {
            Event = ev,
            Categories = await Api.ReadAsync<List<CategoryItem>>(cr) ?? new(),
            Enrolments = await Api.ReadAsync<List<EnrolmentItem>>(enr) ?? new()
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddCategory(int eventId, string categoryName)
    {
        var g = Guard("Organiser"); if (g != null) return g;

        var r = await Api.SendAsync(HttpMethod.Post, $"api/events/{eventId}/categories",
            new CategoryRequest { CategoryName = categoryName });
        if (r.IsSuccessStatusCode) TempData["Success"] = "Category added.";
        else TempData["Error"] = await ApiClient.ErrorAsync(r);
        return RedirectToAction(nameof(Manage), new { id = eventId });
    }

    [HttpPost]
    public async Task<IActionResult> CaptureResult(int eventId, int enrolmentId, string finishTime, int finishPosition)
    {
        var g = Guard("Organiser"); if (g != null) return g;

        var r = await Api.SendAsync(HttpMethod.Post, "api/results",
            new ResultRequest { EnrolmentId = enrolmentId, FinishTime = finishTime, FinishPosition = finishPosition });
        if (r.IsSuccessStatusCode) TempData["Success"] = "Result saved.";
        else TempData["Error"] = await ApiClient.ErrorAsync(r);
        return RedirectToAction(nameof(Manage), new { id = eventId });
    }
}