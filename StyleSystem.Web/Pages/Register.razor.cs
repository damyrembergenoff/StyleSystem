using Microsoft.AspNetCore.Components;
using StyleSystem.Shared.Dtos;
using StyleSystem.Web.Abstractions;

namespace StyleSystem.Web.Pages;

public partial class Register
{
    [Inject] IUserService UserService { get; set; } = default!;
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
            if (registerModel.Password != registerModel.ConfirmPassword)
            {
                errorMessage = "Passwords do not match.";
                isLoading = false;
                return;
            }


            bool isRegistered = await UserService.RegisterAsync(registerModel, cancellationToken: default);

            if(isRegistered)
            {
                successMessage = "Account created successfully! Redirecting to complete profile...";
                Navigation.NavigateTo("complete-profile");
            }
            else
            {
                errorMessage = "Registration failed. Please try again.";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"{ex.Message}, Registration failed. Please try again.";
        }
        finally
        {
            isLoading = false;
        }
    }
}