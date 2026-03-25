using System.ComponentModel.DataAnnotations;

namespace StyleSystem.Web.Dtos.CompleteProfile;

public class ProfileModel
{
    [Required(ErrorMessage = "Full name is required")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Height is required")]
    [Range(100, 250, ErrorMessage = "Height must be between 100 and 250 cm")]
    public int? Height { get; set; }

    [Required(ErrorMessage = "Weight is required")]
    [Range(30, 300, ErrorMessage = "Weight must be between 30 and 300 kg")]
    public int? Weight { get; set; }

    [Required(ErrorMessage = "Age is required")]
    [Range(0, 150, ErrorMessage = "Age must be between 0 and 150")]
    public int? Age { get; set; }

    public string Gender { get; set; } = "";
    public string BodyType { get; set; } = "";
    public string SkinTone { get; set; } = "";
}

public class BodyTypeOption
{
    public string Value { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}

public class SkinToneOption
{
    public string Value { get; set; } = "";
    public string Name { get; set; } = "";
    public string Color { get; set; } = "";
}