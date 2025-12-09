using Model.Models;
using Model.RequestModels;

namespace Service.Interfaces;

public interface IChatService
{
    Task<ChatResponse> AskAsync(ChatRequest request, CancellationToken cancellationToken = default);
    Task<ChatHistoryDto?> GetHistoryAsync(Guid sessionId, CancellationToken cancellationToken = default);
}

