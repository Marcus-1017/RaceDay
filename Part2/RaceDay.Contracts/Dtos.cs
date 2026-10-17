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
