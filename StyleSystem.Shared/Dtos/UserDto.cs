namespace StyleSystem.Shared.Dtos;

public class UserDto
{
    public string? Username { get; set; }
    public string? Password { get; set; }

    public string? FullName { get; set; }
    public int? Age { get; set; }
    public int? Height { get; set; }
    public int? Weight { get; set; }
    public EGender? Gender { get; set; }
    public EMaleBodyType? MaleBodyType { get; set; }
    public EFemaleBodyType? FemaleBodyType { get; set; }
    public ESkinTone? SkinTone { get; set; }
}