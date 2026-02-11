namespace StyleSystem.Api.Dtos;

public class GroqRequest
{
    public string? Model { get; set; }
    public double Temperature { get; set; } = 0.7;
    public List<GroqMessage> Messages { get; set; } = [];
}