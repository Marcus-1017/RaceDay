using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events/{eventId}/categories")]
public class CategoriesController : ApiBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Returns all categories for an event.</summary>
    /// <response code="200">List of categories.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpGet]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventID == eventId))
            return NotFound(new { message = "Event not found." });

        var list = await _db.Category
            .Where(c => c.EventID == eventId)
            .Select(c => new { c.CategoryID, c.CategoryName, c.EventID })
            .ToListAsync();
        return Ok(list);
    }

    /// <summary>Creates a category for one of the Organiser's own events.</summary>
    /// <response code="201">Category created.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or not the owner of the event.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpPost]
    public async Task<IActionResult> Create(int eventId, CategoryRequest request)
    {
        var denied = CheckAccess("Organiser");
        if (denied != null) return denied;

        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserID != CurrentUserId)
            return StatusCode(403, new { message = "You can only add categories to your own events." });

        var category = new Category { CategoryName = request.CategoryName, EventID = eventId };
        _db.Category.Add(category);
        await _db.SaveChangesAsync();

        return StatusCode(201, new { category.CategoryID, category.CategoryName, category.EventID });
    }
}