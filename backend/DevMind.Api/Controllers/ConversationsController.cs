using DevMind.Api.Data;
using DevMind.Api.Dtos;
using DevMind.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevMind.Api.Services;
using System.Text;

namespace DevMind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly OllamaService _ollamaService;

    public ConversationsController(AppDbContext context , OllamaService ollamaService)
    {
        _context = context;
        _ollamaService = ollamaService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ConversationDto>>> GetConversations()
    {
        var conversations = await _context.ChatConversations
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ConversationDto
            {
                Id = c.Id,
                Title = c.Title,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(conversations);
    }

    [HttpGet("{id}/messages")]
    public async Task<ActionResult<List<MessageDto>>> GetMessages(int id)
    {
        var conversationExists = await _context.ChatConversations
            .AnyAsync(c => c.Id == id);

        if (!conversationExists)
        {
            return NotFound("Conversation not found.");
        }

        var messages = await _context.ChatMessages
            .Where(m => m.ChatConversationId == id)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                ChatConversationId = m.ChatConversationId,
                Role = m.Role,
                Content = m.Content,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

        return Ok(messages);
    }

    [HttpPost]
    public async Task<ActionResult<ConversationDto>> CreateConversation(CreateConversationDto dto)
    {
        var conversation = new ChatConversation
        {
            Title = string.IsNullOrWhiteSpace(dto.Title) ? "New Chat" : dto.Title.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.ChatConversations.Add(conversation);
        await _context.SaveChangesAsync();

        var result = new ConversationDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            CreatedAt = conversation.CreatedAt
        };

        return CreatedAtAction(nameof(GetMessages), new { id = conversation.Id }, result);
    }

    [HttpPost("{id}/messages")]
public async Task<ActionResult<List<MessageDto>>> CreateMessage(int id, CreateMessageDto dto)
{
    var conversation = await _context.ChatConversations
        .Include(c => c.Messages)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (conversation == null)
    {
        return NotFound("Conversation not found.");
    }

    if (string.IsNullOrWhiteSpace(dto.Role) || string.IsNullOrWhiteSpace(dto.Content))
    {
        return BadRequest("Role and Content are required.");
    }

    var userMessage = new ChatMessage
    {
        ChatConversationId = id,
        Role = dto.Role.Trim(),
        Content = dto.Content.Trim(),
        CreatedAt = DateTime.UtcNow
    };

    _context.ChatMessages.Add(userMessage);
    await _context.SaveChangesAsync();

    var promptBuilder = new StringBuilder();

    foreach (var msg in conversation.Messages.OrderBy(m => m.CreatedAt))
    {
        promptBuilder.AppendLine($"{msg.Role}: {msg.Content}");
    }

    promptBuilder.AppendLine($"user: {dto.Content}");
    promptBuilder.AppendLine("assistant:");

    var aiReply = await _ollamaService.GenerateReplyAsync(promptBuilder.ToString());

    var assistantMessage = new ChatMessage
    {
        ChatConversationId = id,
        Role = "assistant",
        Content = aiReply.Trim(),
        CreatedAt = DateTime.UtcNow
    };

    _context.ChatMessages.Add(assistantMessage);
    await _context.SaveChangesAsync();

    var result = await _context.ChatMessages
        .Where(m => m.ChatConversationId == id)
        .OrderBy(m => m.CreatedAt)
        .Select(m => new MessageDto
        {
            Id = m.Id,
            ChatConversationId = m.ChatConversationId,
            Role = m.Role,
            Content = m.Content,
            CreatedAt = m.CreatedAt
        })
        .ToListAsync();

    return Ok(result);
}
}