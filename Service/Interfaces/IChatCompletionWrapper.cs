using OpenAI.Chat;

namespace Service.Interfaces;

/// <summary>
/// Wrapper interface for ChatCompletion to enable mocking in tests.
/// ChatCompletion is a sealed class from the OpenAI SDK, so we need this wrapper.
/// </summary>
public interface IChatCompletionWrapper
{
    ChatFinishReason FinishReason { get; }
    ChatMessageContent Content { get; }
    ChatTokenUsage Usage { get; }
    string Model { get; }
    IReadOnlyList<ChatToolCall> ToolCalls { get; }
}

