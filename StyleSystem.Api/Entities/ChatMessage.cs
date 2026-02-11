namespace StyleSystem.Api.Entities;

public class ChatMessage : EntityBase
{
    public EChatRole Role { get; set; }
    public string? Content { get; set; }
    
    public Guid ChatId { get; set; }
    public Chat? Chat { get; set; }
}
