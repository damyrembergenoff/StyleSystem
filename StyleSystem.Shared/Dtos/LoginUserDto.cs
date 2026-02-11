namespace StyleSystem.Shared.Dtos;

public class LoginUserDto
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Username is required")]
    [System.ComponentModel.DataAnnotations.MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    public string? Username { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Password is required")]
    [System.ComponentModel.DataAnnotations.MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string? Password { get; set; }
}