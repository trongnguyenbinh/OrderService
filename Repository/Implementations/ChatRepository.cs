using Domain;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Repository.Implementations;

public class ChatRepository : IChatRepository
{
    private readonly DataContext _context;
    private readonly ILogger<ChatRepository> _logger;

    public ChatRepository(DataContext context, ILogger<ChatRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ChatSessionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting chat session by ID: {SessionId}", id);

        var session = await _context.ChatSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (session == null)
        {
            _logger.LogWarning("Chat session not found with ID: {SessionId}", id);
        }

        return session;
    }

    public async Task<IEnumerable<ChatSessionEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all chat sessions");

        return await _context.ChatSessions
            .AsNoTracking()
            .OrderByDescending(s => s.LastActivityAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatSessionEntity> AddAsync(ChatSessionEntity entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new chat session for fingerprint: {Fingerprint}", entity.UserFingerprint);
        await _context.ChatSessions.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created chat session with ID: {SessionId}", entity.Id);
        return entity;
    }

    public async Task<ChatSessionEntity> UpdateAsync(ChatSessionEntity entity, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating chat session: {SessionId}", entity.Id);

        _context.ChatSessions.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully updated chat session: {SessionId}", entity.Id);
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting chat session: {SessionId}", id);

        var session = await _context.ChatSessions.FindAsync(new object[] { id }, cancellationToken);
        if (session == null)
        {
            _logger.LogWarning("Chat session not found for deletion: {SessionId}", id);
            return false;
        }

        _context.ChatSessions.Remove(session);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully deleted chat session: {SessionId}", id);
        return true;
    }

    public async Task<ChatSessionEntity?> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting chat session by ID: {SessionId}", id);

        var session = await _context.ChatSessions
            .AsNoTracking()
            .Where(s => s.Id == id && s.IsActive)
            .OrderByDescending(s => s.LastActivityAt)
            .FirstOrDefaultAsync(cancellationToken);

        return session;
    }

    public async Task<ChatSessionEntity?> GetSessionByFingerprintAsync(string userFingerprint, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting active chat session for fingerprint: {Fingerprint}", userFingerprint);

        var session = await _context.ChatSessions
            .AsNoTracking()
            .Where(s => s.UserFingerprint == userFingerprint && s.IsActive)
            .OrderByDescending(s => s.LastActivityAt)
            .FirstOrDefaultAsync(cancellationToken);

        return session;
    }

    public async Task<ChatSessionEntity?> GetSessionWithMessagesAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting chat session with messages: {SessionId}", sessionId);

        var session = await _context.ChatSessions
            .AsNoTracking()
            .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

        return session;
    }

    public async Task<List<ChatMessageEntity>> GetSessionMessagesAsync(Guid sessionId, int limit = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting last {Limit} messages for session: {SessionId}", limit, sessionId);

        var messages = await _context.ChatMessages
            .AsNoTracking()
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return messages;
    }

    public async Task<ChatMessageEntity> AddMessageAsync(ChatMessageEntity message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding message to session: {SessionId}, Role: {Role}", message.SessionId, message.Role);

        await _context.ChatMessages.AddAsync(message, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully added message with ID: {MessageId}", message.Id);
        return message;
    }

    public async Task UpdateSessionActivityAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating last activity for session: {SessionId}", sessionId);

        var session = await _context.ChatSessions.FindAsync(new object[] { sessionId }, cancellationToken);
        if (session != null)
        {
            session.LastActivityAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully updated activity for session: {SessionId}", sessionId);
        }
        else
        {
            _logger.LogWarning("Session not found for activity update: {SessionId}", sessionId);
        }
    }

    public async Task<int> GetMessageCountForSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting message count for session: {SessionId}", sessionId);

        var count = await _context.ChatMessages
            .Where(m => m.SessionId == sessionId)
            .CountAsync(cancellationToken);

        return count;
    }
}

