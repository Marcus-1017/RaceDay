using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
public class ResultsController : RaceDay.Api.Controllers.ApiBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db)
    {
        _db = db;
    }

    [HttpPost("/api/results")]
    public async Task<IActionResult> Capture(ResultRequest request)
    {
        var denied = CheckAccess("Organiser");
        if (denied != null) return denied;
        int userId = CurrentUserId!.Value;

        var enrolment = await _db.Enrolment
            .Include(e => e.Event)
            .FirstOrDefaultAsync(e => e.EnrolmentID == request.EnrolmentId);
        if (enrolment == null)
            return BadRequest(new { message = "Enrolment does not exist." });
        if (enrolment.Event.OrganiserID != userId)
            return StatusCode(403, new { message = "You can only capture results for your own events." });

        bool exists = await _db.Result.AnyAsync(r => r.EnrolmentID == request.EnrolmentId);
        if (exists)
            return Conflict(new { message = "A result already exists for this enrolment." });

        var result = new Result
        {
            EnrolmentID = request.EnrolmentId,
            FinishTime = request.FinishTime,
            FinishPosition = request.FinishPosition
        };
        _db.Result.Add(result);
        await _db.SaveChangesAsync();

        return StatusCode(201, new { result.ResultID, result.FinishTime, result.FinishPosition });
    }

    [HttpGet("/api/results/mine")]
    public async Task<IActionResult> Mine()
    {
        var denied = CheckAccess("Participant");
        if (denied != null) return denied;
        int userId = CurrentUserId!.Value;

        var list = await _db.Result
            .Where(r => r.Enrolment.ParticipantID == userId)
            .Select(r => new
            {
                EventName = r.Enrolment.Event.EventName,
                EventDate = r.Enrolment.Event.EventDate,
                CategoryName = r.Enrolment.Category.CategoryName,
                r.FinishTime,
                r.FinishPosition
            })
            .ToListAsync();
        return Ok(list);
    }
}
