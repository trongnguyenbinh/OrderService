using System.ComponentModel.DataAnnotations.Schema;
using Domain.Contractors;

namespace Domain.Entities;

[Table("chat_messages")]
public class ChatMessageEntity : IEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Metadata for analytics
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
    public int? TotalTokens { get; set; }
    public string? ModelUsed { get; set; }
    public string? ToolCalled { get; set; }
    public int? ResponseTimeMs { get; set; }

    // Navigation properties
    public ChatSessionEntity Session { get; set; } = null!;
}

