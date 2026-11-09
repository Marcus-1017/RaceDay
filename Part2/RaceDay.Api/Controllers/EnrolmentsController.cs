using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
public class EnrolmentsController : ApiBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Enrols the logged-in Participant in an event under a chosen category.</summary>
    /// <response code="201">Enrolment recorded.</response>
    /// <response code="400">Category does not belong to that event.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not a Participant.</response>
    /// <response code="409">Already enrolled in this event.</response>
    [HttpPost("/api/enrolments")]
    public async Task<IActionResult> Enrol(EnrolRequest request)
    {
        var denied = CheckAccess("Participant");
        if (denied != null) return denied;
        int userId = CurrentUserId!.Value;

        var category = await _db.Category
            .FirstOrDefaultAsync(c => c.CategoryID == request.CategoryId && c.EventID == request.EventId);
        if (category == null)
            return BadRequest(new { message = "That category does not belong to that event." });

        bool already = await _db.Enrolment
            .AnyAsync(e => e.ParticipantID == userId && e.EventID == request.EventId);
        if (already)
            return Conflict(new { message = "You are already enrolled in this event." });

        var enrolment = new Enrolment
        {
            ParticipantID = userId,
            EventID = request.EventId,
            CategoryID = request.CategoryId
        };
        _db.Enrolment.Add(enrolment);
        await _db.SaveChangesAsync();

        return StatusCode(201, new { enrolment.EnrolmentID, enrolment.EventID, enrolment.CategoryID, enrolment.EnrolmentStatus });
    }

    /// <summary>Returns the logged-in Participant's own enrolments.</summary>
    /// <response code="200">List of enrolments.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not a Participant.</response>
    [HttpGet("/api/enrolments/mine")]
    public async Task<IActionResult> Mine()
    {
        var denied = CheckAccess("Participant");
        if (denied != null) return denied;
        int userId = CurrentUserId!.Value;

        var list = await _db.Enrolment
            .Where(e => e.ParticipantID == userId)
            .Select(e => new
            {
                e.EnrolmentID,
                EventName = e.Event.EventName,
                EventDate = e.Event.EventDate,
                CategoryName = e.Category.CategoryName,
                e.EnrolmentStatus
            })
            .ToListAsync();
        return Ok(list);
    }

    /// <summary>Returns all enrolments for one of the Organiser's own events.</summary>
    /// <response code="200">List of enrolments.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser, or not the owner of the event.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpGet("/api/events/{eventId}/enrolments")]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        var denied = CheckAccess("Organiser");
        if (denied != null) return denied;

        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserID != CurrentUserId)
            return StatusCode(403, new { message = "You can only view enrolments for your own events." });

        var list = await _db.Enrolment
            .Where(e => e.EventID == eventId)
            .Select(e => new
            {
                e.EnrolmentID,
                ParticipantName = e.Participant.FullName,
                CategoryName = e.Category.CategoryName,
                e.EnrolmentStatus
            })
            .ToListAsync();
        return Ok(list);
    }
}