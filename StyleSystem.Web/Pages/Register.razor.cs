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
                errorMessage = "Parollar sáykes kelmeydi.";
                isLoading = false;
                return;
            }


            bool isRegistered = await UserService.RegisterAsync(registerModel, cancellationToken: default);

            if(isRegistered)
            {
                successMessage = "Akkount jaratıldı! Profildi toltırıwǵa ótkerilmekte...";
                Navigation.NavigateTo("complete-profile");
            }
            else
            {
                errorMessage = "Registraciya ámelge aspadı. Iltimas qayta urınıń.";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"{ex.Message}, Registraciya ámelge aspadı. Iltimas qayta urınıń.";
        }
        finally
        {
            isLoading = false;
        }
    }
}