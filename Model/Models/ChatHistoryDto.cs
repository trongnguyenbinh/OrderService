namespace Model.Models;

public class ChatHistoryDto
{
    public Guid SessionId { get; set; }
    public string UserFingerprint { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = new();
}

