using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using StyleSystem.Web.Abstractions;
using StyleSystem.Web.Auth;
using StyleSystem.Web.Dtos.CompleteProfile;

namespace StyleSystem.Web.Pages;
public partial class CompleteProfile
{
    [Inject] public required IUserService UserService { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }
    [Inject] public required AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] public required ILogger<CompleteProfile> Logger { get; set; }
    private ProfileModel profileModel = new();
    private int currentStep = 1;
    private string errorMessage = "";
    private string successMessage = "";
    private bool isLoading = false;

    protected override async Task OnInitializedAsync()
    {
        var customAuth = (CustomAuthStateProvider)AuthStateProvider;
        var authState = await customAuth.GetAuthenticationStateAsync();
        var user = authState.User;


        if(!user.Identity?.IsAuthenticated ?? true)
        {
            Navigation.NavigateTo("login");
            return;
        }

        profileModel.FullName = user?.Claims?.FirstOrDefault(c => c.Type.Equals("FullName"))?.Value ?? "";
    }

    private List<BodyTypeOption> maleBodyTypes = new()
    {
        new BodyTypeOption 
        { 
            Value = "Rectangle", 
            Name = "Rectangle", 
            Description = "Dúziw dene, bel hám iyin keńligi birdey."
        },
        new BodyTypeOption 
        { 
            Value = "Triangle", 
            Name = "Triangle", 
            Description = "Jambasları keńirek, iyinleri tar"
        },
        new BodyTypeOption 
        { 
            Value = "InvertedTriangle", 
            Name = "Inverted Triangle", 
            Description = "Iyinleri keń, beli tar."
        },
        new BodyTypeOption 
        { 
            Value = "Trapezoid", 
            Name = "Trapezoid", 
            Description = "Atletik deneli, iyinleri jambasqa qaraganda biraz keńirek."
        },
        new BodyTypeOption 
        { 
            Value = "Oval", 
            Name = "Oval", 
            Description = "Orta bólimi domalaq, ayaq-qolları jińishke."
        }
    };

    private List<BodyTypeOption> femaleBodyTypes = new()
    {
        new BodyTypeOption 
        { 
            Value = "Hourglass", 
            Name = "Hourglass", 
            Description = "Balanstırılǵan kókirek hám jambas, bel belgilengen."
        },
        new BodyTypeOption 
        { 
            Value = "Pear", 
            Name = "Pear", 
            Description = "jambas keńirek, kókirek kishirek"
        },
        new BodyTypeOption 
        { 
            Value = "Apple", 
            Name = "Apple", 
            Description = "Orta bólimi keń, ayaqları jinishke."
        },
        new BodyTypeOption 
        { 
            Value = "Rectangle", 
            Name = "Rectangle", 
            Description = "Tuwrı figura, uqsas ólshemler"
        },
        new BodyTypeOption 
        { 
            Value = "InvertedTriangle", 
            Name = "Inverted Triangle", 
            Description = "Keń jawırın, tar jambas."
        },
        new BodyTypeOption 
        { 
            Value = "Diamond", 
            Name = "Diamond", 
            Description = "Orta bólimi keńirek, tóbesi hám astı jińishke."
        }
    };

    // Skin Tones
    private List<SkinToneOption> skinTones = new()
    {
        new SkinToneOption { Value = "Fair", Name = "Aq", Color = "#FFE0BD" },
        new SkinToneOption { Value = "Light", Name = "Ashıq", Color = "#F1C27D" },
        new SkinToneOption { Value = "Medium", Name = "Ortasha", Color = "#C68642" },
        new SkinToneOption { Value = "Olive", Name = "Zaytun", Color = "#8D5524" },
        new SkinToneOption { Value = "Tan", Name = "qońır", Color = "#704214" },
        new SkinToneOption { Value = "Dark", Name = "Qara", Color = "#4A2511" }
    };

    private void SelectMaleGender() => SelectGender("Male");
    private void SelectFemaleGender() => SelectGender("Female");

    private void SelectGender(string gender)
    {
        profileModel.Gender = gender;
        profileModel.BodyType = ""; // Reset body type when gender changes
    }

    private void SelectBodyType(string bodyType)
    {
        profileModel.BodyType = bodyType;
    }

    private void SelectSkinTone(string skinTone)
    {
        profileModel.SkinTone = skinTone;
    }

    private void NextStep()
    {
        errorMessage = "";

        // Validate current step
        if (currentStep == 1)
        {
            if (string.IsNullOrEmpty(profileModel.FullName) || 
                !profileModel.Height.HasValue || 
                !profileModel.Weight.HasValue)
            {
                errorMessage = "Iltimas barlıq maydanlardı toltırıń";
                return;
            }
        }
        else if (currentStep == 2)
        {
            if (string.IsNullOrEmpty(profileModel.Gender))
            {
                errorMessage = "Iltimas jınısıńızdı tańlań";
                return;
            }
        }
        else if (currentStep == 3)
        {
            if (string.IsNullOrEmpty(profileModel.BodyType))
            {
                errorMessage = "Iltimas dene túrin tańlań.";
                return;
            }
        }

        if (currentStep < 4)
        {
            currentStep++;
        }
    }

    private void PreviousStep()
    {
        if (currentStep > 1)
        {
            currentStep--;
        }
    }

    private string GetProgressPercentage()
    {
        return ((currentStep / 4.0) * 100).ToString("0");
    }

    private async Task HandleSubmit()
    {
        errorMessage = "";
        successMessage = "";

        // Final validation
        if (string.IsNullOrEmpty(profileModel.SkinTone))
        {
            errorMessage = "Iltimas teri reńin tańlań.";
            return;
        }

        isLoading = true;

        try
        {
            Logger.LogInformation("Submitting profile data: {FullName}, {Height}, {Weight}, {Gender}, {BodyType}, {SkinTone}", 
                profileModel.FullName, 
                profileModel.Height, 
                profileModel.Weight, 
                profileModel.Gender, 
                profileModel.BodyType, 
                profileModel.SkinTone);
            // TODO: Send data to backend API
            var isSuccess = await UserService.UpdateUserAsync(profileModel);
            if (isSuccess)
            {
                successMessage = "Profil tabıslı toltırıldı! Redirecting...";
                Navigation.NavigateTo("dashboard");
            }

            else
            {
                errorMessage = "Profil saqlanbadı. Iltimas qayta urınıń.";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"{ex.Message} Profil saqlanbadı. Iltimas qayta urınıń.";
        }
        finally
        {
            isLoading = false;
        }
    }
}