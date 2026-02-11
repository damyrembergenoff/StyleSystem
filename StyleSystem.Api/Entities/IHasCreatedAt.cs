namespace StyleSystem.Api.Entities;

public interface IHasCreatedAt
{
    DateTimeOffset CreatedAt { get; set; }
}