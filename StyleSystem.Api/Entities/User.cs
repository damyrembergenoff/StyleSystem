namespace StyleSystem.Api.Entities;

public class User : EntityBase
{
    public string? FullName { get; set; }
    public string? Username { get; set; }
    public string? PasswordHash { get; set; }

    public int? Height { get; set; }
    public int? Weight { get; set; }
    public EGender? Gender { get; set; }
    public EMaleBodyType? MaleBodyType { get; set; }
    public EFemaleBodyType? FemaleBodyType { get; set; }
    public ESkinTone? SkinTone { get; set; }

    public IList<FashionRecommendation> Recommendations { get; set; } = [];
    public IList<Chat> Chats { get; set; } = [];
}