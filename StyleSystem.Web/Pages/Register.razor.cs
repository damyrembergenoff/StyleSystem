using StyleSystem.Shared.Dtos;

namespace StyleSystem.Web.Pages;

public partial class Register
{
    private RegisterUserDto registerModel = new();
    private string errorMessage = "";
    private string successMessage = "";
    private bool isLoading = false;

    private async Task HandleRegister()
    {
        isLoading = true;
        errorMessage = "";
        successMessage = "";

        try
        {
            // Validate passwords match
            if (registerModel.Password != registerModel.ConfirmPassword)
            {
                errorMessage = "Passwords do not match.";
                isLoading = false;
                return;
            }

            // TODO: Implement actual registration logic with your API
            // Example:
            // var response = await AuthService.RegisterAsync(registerModel);
            
            // Simulate API call
            await Task.Delay(1500);

            // TODO: Replace with real registration
            // If successful:
            // successMessage = "Account created successfully! Redirecting...";
            // await Task.Delay(2000);
            // Navigation.NavigateTo("/login");

            // For now, show success and redirect
            successMessage = "Account created successfully! Redirecting to login...";
            await Task.Delay(2000);
            Navigation.NavigateTo("/login");
        }
        catch (Exception ex)
        {
            errorMessage = "Registration failed. Please try again.";
        }
        finally
        {
            isLoading = false;
        }
    }

    // public class RegisterModel
    // {
    //     [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Full name is required")]
    //     [System.ComponentModel.DataAnnotations.MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
    //     public string FullName { get; set; } = "";

    //     [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Username is required")]
    //     [System.ComponentModel.DataAnnotations.MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    //     [System.ComponentModel.DataAnnotations.RegularExpression(@"^[a-zA-Z0-9_]+$", 
    //         ErrorMessage = "Username can only contain letters, numbers, and underscores")]
    //     public string Username { get; set; } = "";

    //     [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Password is required")]
    //     [System.ComponentModel.DataAnnotations.MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    //     public string Password { get; set; } = "";

    //     [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Please confirm your password")]
    //     public string ConfirmPassword { get; set; } = "";

    //     [System.ComponentModel.DataAnnotations.Range(typeof(bool), "true", "true", 
    //         ErrorMessage = "You must accept the terms and conditions")]
    //     public bool AcceptTerms { get; set; }
    // }
}