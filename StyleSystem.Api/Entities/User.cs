using System.ComponentModel.DataAnnotations;

namespace StyleSystem.Api.Entities;

public class User : EntityBase
{
    public string? FullName { get; set; }
    public string? Username { get; set; }
    public string? PasswordHash { get; set; }

    [Range(0, 120)]
    public int? Age { get; set; }
    public int? Height { get; set; }
    public int? Weight { get; set; }
    public EGender? Gender { get; set; }
    public EMaleBodyType? MaleBodyType { get; set; }
    public EFemaleBodyType? FemaleBodyType { get; set; }
    public ESkinTone? SkinTone { get; set; }

    public IList<Recommendation> Recommendations { get; set; } = [];
}