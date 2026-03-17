namespace DevMind.Api.Dtos;

public class MessageDto
{
    public int Id { get; set; }
    public int ChatConversationId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}