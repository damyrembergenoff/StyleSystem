using Microsoft.AspNetCore.Components;
using StyleSystem.Shared.Dtos;
using StyleSystem.Web.Abstractions;

namespace StyleSystem.Web.Pages;

public partial class Login
{
    [Inject] private IUserService UserService { get; set; } = default!;
    private LoginUserDto loginModel = new();
    private string errorMessage = "";
    private bool isLoading = false;

    private async Task HandleLogin()
    {
        isLoading = true;
        errorMessage = "";

        if (string.IsNullOrWhiteSpace(loginModel.Username) || string.IsNullOrWhiteSpace(loginModel.Password))
        {
            errorMessage = "Barlıq maydanlardı toltırıń.";
            isLoading = false;
            return;
        }

        var isSuccess = await UserService.LoginAsync(loginModel);

        if(isSuccess is true)
        {
            Navigation.NavigateTo("dashboard");
        }
        else
        {
            errorMessage = "Username yamasa parol nadurıs.";
        }

        isLoading = false;
    }
}