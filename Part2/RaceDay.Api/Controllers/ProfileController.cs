using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/profile")]
public class ProfileController : ApiBase
{
    private readonly RaceDayDbContext _db;

    public ProfileController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Returns the logged-in user's own profile.</summary>
    /// <response code="200">The profile.</response>
    /// <response code="401">Not logged in.</response>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var denied = CheckAccess();
        if (denied != null) return denied;

        var user = await _db.Users.FindAsync(CurrentUserId!.Value);
        if (user == null) return NotFound(new { message = "User not found." });

        return Ok(new { user.UserID, user.FullName, user.Email, user.Role });
    }

    /// <summary>Updates the logged-in user's own profile.</summary>
    /// <response code="200">Profile updated.</response>
    /// <response code="400">Invalid data or email already used.</response>
    /// <response code="401">Not logged in.</response>
    [HttpPut]
    public async Task<IActionResult> Update(UpdateProfileRequest request)
    {
        var denied = CheckAccess();
        if (denied != null) return denied;

        var user = await _db.Users.FindAsync(CurrentUserId!.Value);
        if (user == null) return NotFound(new { message = "User not found." });

        bool taken = await _db.Users.AnyAsync(u => u.Email == request.Email && u.UserID != user.UserID);
        if (taken) return BadRequest(new { message = "That email is already used." });

        user.FullName = request.FullName;
        user.Email = request.Email;
        await _db.SaveChangesAsync();

        return Ok(new { user.UserID, user.FullName, user.Email, user.Role });
    }
}