namespace DevMind.Api.Models;

public class ChatConversation
{
    public int Id { get; set; }

    public string Title { get; set; } = "New Chat";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<ChatMessage> Messages { get; set; } = new();
}