namespace StyleSystem.Api.Entities;

public class EntityBase : IHasCreatedAt
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
