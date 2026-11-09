using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public AuthController(RaceDayDbContext db)
    {
        _db = db;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (request.Role != "Organiser" && request.Role != "Participant")
            return BadRequest(new { message = "Role must be Organiser or Participant." });

        bool emailTaken = await _db.Users.AnyAsync(u => u.Email == request.Email);
        if (emailTaken)
            return BadRequest(new { message = "That email is already registered." });

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return StatusCode(201, new AuthResponse
        {
            UserID = user.UserID,
            FullName = user.FullName,
            Role = user.Role
        });
    }

    /// <summary>Logs a user in and starts a session storing their user ID and role.</summary>
    /// <response code="200">Login successful.</response>
    /// <response code="401">Wrong email or password.</response>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("Role", user.Role);

        return Ok(new AuthResponse { UserID = user.UserID, FullName = user.FullName, Role = user.Role });
    }

    /// <summary>Ends the current session.</summary>
    /// <response code="200">Logged out.</response>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logged out." });
    }
}
