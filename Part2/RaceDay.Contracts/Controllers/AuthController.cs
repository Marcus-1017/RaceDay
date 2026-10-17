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

    /// <summary>Registers a new user as either an Organiser or a Participant.</summary>
    /// <response code="201">User created.</response>
    /// <response code="400">Invalid data, invalid role, or email already registered.</response>
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
}