using OpenAI.Chat;

namespace Service.Interfaces;

public interface IOpenAIService
{
    Task<ChatCompletion> GetChatCompletionAsync(
        List<ChatMessage> messages,
        ChatCompletionOptions? options = null,
        CancellationToken cancellationToken = default);

    Task<ChatCompletion> GetChatCompletionWithToolsAsync(
        List<ChatMessage> messages,
        IEnumerable<ChatTool> tools,
        ChatCompletionOptions? options = null,
        CancellationToken cancellationToken = default);
}

