using Domain.Contractors;
using Domain.Entities;

namespace Domain.Interfaces.Repositories;

public interface IChatRepository : IRepository<ChatSessionEntity, Guid>
{
    Task<ChatSessionEntity?> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ChatSessionEntity?> GetSessionByFingerprintAsync(string userFingerprint, CancellationToken cancellationToken = default);
    Task<ChatSessionEntity?> GetSessionWithMessagesAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<List<ChatMessageEntity>> GetSessionMessagesAsync(Guid sessionId, int limit = 10, CancellationToken cancellationToken = default);
    Task<ChatMessageEntity> AddMessageAsync(ChatMessageEntity message, CancellationToken cancellationToken = default);
    Task UpdateSessionActivityAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<int> GetMessageCountForSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
}

