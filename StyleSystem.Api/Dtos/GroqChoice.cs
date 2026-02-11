namespace StyleSystem.Api.Dtos;

public class GroqChoice
{
    public GroqMessage? Message { get; set; }
    public string? FinishReason { get; set; }
}