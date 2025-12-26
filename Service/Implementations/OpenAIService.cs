using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using Service.Interfaces;

namespace Service.Implementations;

public class OpenAIService : IOpenAIService
{
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<OpenAIService> _logger;
    private readonly string _modelName;

    public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
    {
        _logger = logger;

        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("OpenAI API key is not configured");
            throw new InvalidOperationException("OpenAI API key is not configured. Please set OpenAI:ApiKey in configuration.");
        }

        _modelName = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
        _openAIClient = new OpenAIClient(apiKey);

        _logger.LogInformation("OpenAI service initialized with model: {Model}", _modelName);
    }

    public async Task<ChatCompletion> GetChatCompletionAsync(
        List<ChatMessage> messages,
        ChatCompletionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Requesting chat completion with {MessageCount} messages", messages.Count);

        var chatOptions = options ?? new ChatCompletionOptions
        {
            Temperature = 0.7f,
            MaxOutputTokenCount = 1000
        };

        var chatClient = _openAIClient.GetChatClient(_modelName);
        var completion = await chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken);

        _logger.LogInformation(
            "Chat completion successful. Tokens - Prompt: {PromptTokens}, Completion: {CompletionTokens}, Total: {TotalTokens}",
            completion.Value.Usage.InputTokenCount,
            completion.Value.Usage.OutputTokenCount,
            completion.Value.Usage.TotalTokenCount);

        return completion.Value;
    }

    public async Task<ChatCompletion> GetChatCompletionWithToolsAsync(
        List<ChatMessage> messages,
        IEnumerable<ChatTool> tools,
        ChatCompletionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Requesting chat completion with tools. Messages: {MessageCount}, Tools: {ToolCount}",
            messages.Count, tools.Count());

        var chatOptions = options ?? new ChatCompletionOptions
        {
            Temperature = 0.7f,
            MaxOutputTokenCount = 1000
        };

        // Add tools to options
        foreach (var tool in tools)
        {
            chatOptions.Tools.Add(tool);
        }

        var chatClient = _openAIClient.GetChatClient(_modelName);
        var completion = await chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken);

        _logger.LogInformation(
            "Chat completion with tools successful. Tokens - Prompt: {PromptTokens}, Completion: {CompletionTokens}, Total: {TotalTokens}",
            completion.Value.Usage.InputTokenCount,
            completion.Value.Usage.OutputTokenCount,
            completion.Value.Usage.TotalTokenCount);

        return completion.Value;
    }
}

