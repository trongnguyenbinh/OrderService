using System.ComponentModel.DataAnnotations.Schema;
using Domain.Contractors;

namespace Domain.Entities;

[Table("chat_sessions")]
public class ChatSessionEntity : IEntity<Guid>
{
    public Guid Id { get; set; }
    public string UserFingerprint { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public ICollection<ChatMessageEntity> Messages { get; set; } = new List<ChatMessageEntity>();
}

