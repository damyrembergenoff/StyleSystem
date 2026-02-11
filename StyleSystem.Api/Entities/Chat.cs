namespace StyleSystem.Api.Entities;

public class Chat : EntityBase
{
    public sbyte? Title { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public IList<ChatMessage> Messages = [];
}