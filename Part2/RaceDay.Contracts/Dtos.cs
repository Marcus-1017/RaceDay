using System.ComponentModel.DataAnnotations;

namespace RaceDay.Contracts;

public class RegisterRequest
{
    [Required]
    public string FullName { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, MinLength(6)]
    public string Password { get; set; } = "";

    [Required]
    public string Role { get; set; } = "";
}

public class LoginRequest
{
    [Required]
    public string Email { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

public class AuthResponse
{
    public int UserID { get; set; }
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
}

public class UpdateProfileRequest
{
    [Required] public string FullName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
}

public class EventRequest
{
    [Required] public string EventName { get; set; } = "";
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    [Required] public string Location { get; set; } = "";
    public decimal? Distance { get; set; }
    [Required] public string EventType { get; set; } = "";
}

public class CategoryRequest
{
    [Required] public string CategoryName { get; set; } = "";
}

public class EnrolRequest
{
    public int EventId { get; set; }
    public int CategoryId { get; set; }
}

public class ResultRequest
{
    public int EnrolmentId { get; set; }
    [Required] public string FinishTime { get; set; } = "";
    public int FinishPosition { get; set; }
}