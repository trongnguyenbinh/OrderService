using System.ComponentModel.DataAnnotations;

namespace Model.RequestModels;

public class ChatRequest
{
    [Required(ErrorMessage = "User fingerprint is required")]
    [MaxLength(500, ErrorMessage = "User fingerprint cannot exceed 500 characters")]
    public string UserFingerprint { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required")]
    [MaxLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
    public string Message { get; set; } = string.Empty;

    public Guid? SessionId { get; set; }
}

