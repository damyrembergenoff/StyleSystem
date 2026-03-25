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
            Navigation.NavigateTo("/login");
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
            Description = "Straight body, similar waist and shoulder width"
        },
        new BodyTypeOption 
        { 
            Value = "Triangle", 
            Name = "Triangle", 
            Description = "Wider hips, narrower shoulders"
        },
        new BodyTypeOption 
        { 
            Value = "InvertedTriangle", 
            Name = "Inverted Triangle", 
            Description = "Broad shoulders, narrow waist"
        },
        new BodyTypeOption 
        { 
            Value = "Trapezoid", 
            Name = "Trapezoid", 
            Description = "Athletic build, shoulders slightly wider than hips"
        },
        new BodyTypeOption 
        { 
            Value = "Oval", 
            Name = "Oval", 
            Description = "Round midsection, slim limbs"
        }
    };

    private List<BodyTypeOption> femaleBodyTypes = new()
    {
        new BodyTypeOption 
        { 
            Value = "Hourglass", 
            Name = "Hourglass", 
            Description = "Balanced bust and hips, defined waist"
        },
        new BodyTypeOption 
        { 
            Value = "Pear", 
            Name = "Pear", 
            Description = "Wider hips, smaller bust"
        },
        new BodyTypeOption 
        { 
            Value = "Apple", 
            Name = "Apple", 
            Description = "Wider midsection, slim legs"
        },
        new BodyTypeOption 
        { 
            Value = "Rectangle", 
            Name = "Rectangle", 
            Description = "Straight figure, similar measurements"
        },
        new BodyTypeOption 
        { 
            Value = "InvertedTriangle", 
            Name = "Inverted Triangle", 
            Description = "Broad shoulders, narrow hips"
        },
        new BodyTypeOption 
        { 
            Value = "Diamond", 
            Name = "Diamond", 
            Description = "Wider midsection, slimmer top and bottom"
        }
    };

    // Skin Tones
    private List<SkinToneOption> skinTones = new()
    {
        new SkinToneOption { Value = "Fair", Name = "Fair", Color = "#FFE0BD" },
        new SkinToneOption { Value = "Light", Name = "Light", Color = "#F1C27D" },
        new SkinToneOption { Value = "Medium", Name = "Medium", Color = "#C68642" },
        new SkinToneOption { Value = "Olive", Name = "Olive", Color = "#8D5524" },
        new SkinToneOption { Value = "Tan", Name = "Tan", Color = "#704214" },
        new SkinToneOption { Value = "Dark", Name = "Dark", Color = "#4A2511" }
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
                errorMessage = "Please fill in all fields";
                return;
            }
        }
        else if (currentStep == 2)
        {
            if (string.IsNullOrEmpty(profileModel.Gender))
            {
                errorMessage = "Please select your gender";
                return;
            }
        }
        else if (currentStep == 3)
        {
            if (string.IsNullOrEmpty(profileModel.BodyType))
            {
                errorMessage = "Please select your body type";
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
            errorMessage = "Please select your skin tone";
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
                successMessage = "Profile completed successfully! Redirecting...";
                Navigation.NavigateTo("/dashboard");
            }

            else
            {
                errorMessage = "Failed to save profile. Please try again.";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"{ex.Message} Failed to save profile. Please try again.";
        }
        finally
        {
            isLoading = false;
        }
    }
}