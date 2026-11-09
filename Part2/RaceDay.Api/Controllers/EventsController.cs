using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ApiBase
{
    private readonly RaceDayDbContext _db;
    private static readonly string[] ValidTypes = { "Run", "Walk", "Cycle" };

    public EventsController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Returns all RaceDay events.</summary>
    /// <response code="200">List of events.</response>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var events = await _db.Events
            .Select(e => new { e.EventID, e.EventName, e.Description, e.EventDate, e.Location, e.Distance, e.EventType, e.OrganiserID })
            .ToListAsync();
        return Ok(events);
    }

    /// <summary>Returns one event by ID.</summary>
    /// <response code="200">The event.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var ev = await _db.Events
            .Where(e => e.EventID == id)
            .Select(e => new { e.EventID, e.EventName, e.Description, e.EventDate, e.Location, e.Distance, e.EventType, e.OrganiserID })
            .FirstOrDefaultAsync();
        if (ev == null) return NotFound(new { message = "Event not found." });
        return Ok(ev);
    }

    /// <summary>Creates a new event (Organiser only).</summary>
    /// <response code="201">Event created.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser.</response>
    [HttpPost]
    public async Task<IActionResult> Create(EventRequest request)
    {
        var denied = CheckAccess("Organiser");
        if (denied != null) return denied;

        if (!ValidTypes.Contains(request.EventType))
            return BadRequest(new { message = "EventType must be Run, Walk or Cycle." });

        var ev = new Event
        {
            EventName = request.EventName,
            Description = request.Description,
            EventDate = request.EventDate,
            Location = request.Location,
            Distance = request.Distance,
            EventType = request.EventType,
            OrganiserID = CurrentUserId!.Value
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        return StatusCode(201, new { ev.EventID, ev.EventName });
    }

    /// <summary>Updates one of the Organiser's own events.</summary>
    /// <response code="200">Event updated.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or not the owner.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, EventRequest request)
    {
        var denied = CheckAccess("Organiser");
        if (denied != null) return denied;

        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserID != CurrentUserId)
            return StatusCode(403, new { message = "You can only edit your own events." });

        if (!ValidTypes.Contains(request.EventType))
            return BadRequest(new { message = "EventType must be Run, Walk or Cycle." });

        ev.EventName = request.EventName;
        ev.Description = request.Description;
        ev.EventDate = request.EventDate;
        ev.Location = request.Location;
        ev.Distance = request.Distance;
        ev.EventType = request.EventType;
        await _db.SaveChangesAsync();

        return Ok(new { ev.EventID, ev.EventName });
    }

    /// <summary>Deletes one of the Organiser's own events.</summary>
    /// <response code="204">Event deleted.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or not the owner.</response>
    /// <response code="404">Event does not exist.</response>
    /// <response code="409">Event still has categories or enrolments.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var denied = CheckAccess("Organiser");
        if (denied != null) return denied;

        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserID != CurrentUserId)
            return StatusCode(403, new { message = "You can only delete your own events." });

        try
        {
            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Remove this event's categories and enrolments first." });
        }

        return NoContent();
    }
}