namespace StyleSystem.Shared.Dtos;

public class RegisterUserDto
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Full name is required")]
    [System.ComponentModel.DataAnnotations.MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
    public string? FullName { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Username is required")]
    [System.ComponentModel.DataAnnotations.MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [System.ComponentModel.DataAnnotations.RegularExpression(@"^[a-zA-Z0-9_]+$", 
        ErrorMessage = "Username can only contain letters, numbers, and underscores")]
    public string? Username { get; set; }
        
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Password is required")]
    [System.ComponentModel.DataAnnotations.MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string? Password { get; set; }

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Please confirm your password")]
    public string ConfirmPassword { get; set; } = "";
}
