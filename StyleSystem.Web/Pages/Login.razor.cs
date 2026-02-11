using StyleSystem.Shared.Dtos;

namespace StyleSystem.Web.Pages;

public partial class Login
{
    private LoginUserDto loginModel = new();
    private string errorMessage = "";
    private bool isLoading = false;

    private async Task HandleLogin()
    {
        isLoading = true;
        errorMessage = "";

        try
        {
            // TODO: Implement actual login logic with your API
            // Example:
            // var response = await AuthService.LoginAsync(loginModel);
            
            // Simulate API call
            await Task.Delay(1000);

            // Temporary validation
            if (string.IsNullOrWhiteSpace(loginModel.Username) || string.IsNullOrWhiteSpace(loginModel.Password))
            {
                errorMessage = "Please fill in all fields.";
                isLoading = false;
                return;
            }

            // TODO: Replace with real authentication
            // If successful:
            // await LocalStorage.SetItemAsync("authToken", response.Token);
            // Navigation.NavigateTo("/dashboard");

            // For now, just navigate
            Navigation.NavigateTo("/dashboard");
        }
        catch (Exception ex)
        {
            errorMessage = "Login failed. Please check your credentials.";
        }
        finally
        {
            isLoading = false;
        }
    }
}