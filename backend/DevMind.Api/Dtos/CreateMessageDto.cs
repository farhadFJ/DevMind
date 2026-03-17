namespace DevMind.Api.Dtos;

public class CreateMessageDto
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}