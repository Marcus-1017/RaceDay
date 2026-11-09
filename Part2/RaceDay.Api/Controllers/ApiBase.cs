using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Api.Controllers;

public abstract class ApiBase : ControllerBase
{
    // read from the session that login created
    protected int? CurrentUserId => HttpContext.Session.GetInt32("UserID");
    protected string? CurrentRole => HttpContext.Session.GetString("Role");

    // returns an error if access is denied, or null if the request may continue
    protected IActionResult? CheckAccess(string? requiredRole = null)
    {
        if (CurrentUserId == null)
            return Unauthorized(new { message = "You must be logged in." });

        if (requiredRole != null && CurrentRole != requiredRole)
            return StatusCode(403, new { message = "Only " + requiredRole + "s can do this." });

        return null;
    }
}
