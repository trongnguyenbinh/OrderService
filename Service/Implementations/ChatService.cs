using System.Diagnostics;
using System.Text.Json;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model.Models;
using Model.RequestModels;
using OpenAI.Chat;
using Repository.Interfaces;
using Service.Interfaces;

namespace Service.Implementations;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOpenAIService _openAIService;
    private readonly IMapper _mapper;
    private readonly ILogger<ChatService> _logger;
    private readonly int _maxMessagesPerSession;
    private readonly int _maxMessageLength;

    public ChatService(
        IChatRepository chatRepository,
        IProductRepository productRepository,
        IOpenAIService openAIService,
        IMapper mapper,
        ILogger<ChatService> logger,
        IConfiguration configuration)
    {
        _chatRepository = chatRepository;
        _productRepository = productRepository;
        _openAIService = openAIService;
        _mapper = mapper;
        _logger = logger;
        _maxMessagesPerSession = int.TryParse(configuration["Chat:MaxMessagesPerSession"], out var maxMessages) ? maxMessages : 10;
        _maxMessageLength = int.TryParse(configuration["Chat:MaxMessageLength"], out var maxLength) ? maxLength : 2000;
    }

    public async Task<ChatResponse> AskAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing chat request for fingerprint: {Fingerprint}", request.UserFingerprint);

        // Validate message length
        if (request.Message.Length > _maxMessageLength)
        {
            throw new ArgumentException($"Message exceeds maximum length of {_maxMessageLength} characters");
        }

        var stopwatch = Stopwatch.StartNew();

        // Get or create session
        var session = await GetOrCreateSessionAsync(request.UserFingerprint, request.SessionId, cancellationToken);

        // Save user message
        var userMessage = new ChatMessageEntity
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            Role = "user",
            Content = request.Message,
            CreatedAt = DateTime.UtcNow
        };
        await _chatRepository.AddMessageAsync(userMessage, cancellationToken);

        // Get conversation history
        var conversationHistory = await _chatRepository.GetSessionMessagesAsync(
            session.Id,
            _maxMessagesPerSession * 2, // Get last 10 messages (user + assistant pairs)
            cancellationToken);

        // Build messages for OpenAI
        var messages = BuildChatMessages(conversationHistory);

        // Define tools for function calling
        var tools = GetChatTools();

        // Get AI response with tools
        var completion = await _openAIService.GetChatCompletionWithToolsAsync(
            messages,
            tools,
            new ChatCompletionOptions
            {
                Temperature = 0.7f,
                MaxOutputTokenCount = 1000
            },
            cancellationToken);

        stopwatch.Stop();

        string assistantResponse;
        string? toolCalled = null;

        //Check if AI wants to call a tool
        if (completion.FinishReason == ChatFinishReason.ToolCalls && completion.ToolCalls.Count > 0)
        {
            _logger.LogInformation("AI requested tool call: {ToolName}", completion.ToolCalls[0].FunctionName);
            toolCalled = completion.ToolCalls[0].FunctionName;

            // Execute the tool and get result
            var toolResult = await ExecuteToolAsync(completion.ToolCalls[0], cancellationToken);

            // Add tool call and result to messages
            messages.Add(new AssistantChatMessage(completion));
            messages.Add(new ToolChatMessage(completion.ToolCalls[0].Id, toolResult));

            // Get final response from AI with tool result
            var finalCompletion = await _openAIService.GetChatCompletionAsync(
                messages,
                new ChatCompletionOptions
                {
                    Temperature = 0.7f,
                    MaxOutputTokenCount = 1000
                },
                cancellationToken);

            assistantResponse = finalCompletion.Content[0].Text;

            // Save assistant message with metadata
            var assistantMessage = new ChatMessageEntity
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                Role = "assistant",
                Content = assistantResponse,
                CreatedAt = DateTime.UtcNow,
                PromptTokens = completion.Usage.InputTokenCount + finalCompletion.Usage.InputTokenCount,
                CompletionTokens = completion.Usage.OutputTokenCount + finalCompletion.Usage.OutputTokenCount,
                TotalTokens = completion.Usage.TotalTokenCount + finalCompletion.Usage.TotalTokenCount,
                ModelUsed = completion.Model,
                ToolCalled = toolCalled,
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
            };
            await _chatRepository.AddMessageAsync(assistantMessage, cancellationToken);
        }
        else
        {
            // No tool call, use direct response
            assistantResponse = completion.Content[0].Text;

            // Save assistant message with metadata
            var assistantMessage = new ChatMessageEntity
            {
                Id = Guid.NewGuid(),
                SessionId = session.Id,
                Role = "assistant",
                Content = assistantResponse,
                CreatedAt = DateTime.UtcNow,
                PromptTokens = completion.Usage.InputTokenCount,
                CompletionTokens = completion.Usage.OutputTokenCount,
                TotalTokens = completion.Usage.TotalTokenCount,
                ModelUsed = completion.Model,
                ToolCalled = toolCalled,
                ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
            };
            await _chatRepository.AddMessageAsync(assistantMessage, cancellationToken);
        }

        // Update session activity
        await _chatRepository.UpdateSessionActivityAsync(session.Id, cancellationToken);

        _logger.LogInformation(
            "Chat request completed. Session: {SessionId}, ResponseTime: {ResponseTime}ms, Tokens: {TotalTokens}",
            session.Id, stopwatch.ElapsedMilliseconds, completion.Usage.TotalTokenCount);

        return new ChatResponse
        {
            SessionId = session.Id,
            Message = assistantResponse,
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<ChatHistoryDto?> GetHistoryAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting chat history for session: {SessionId}", sessionId);

        var session = await _chatRepository.GetSessionWithMessagesAsync(sessionId, cancellationToken);
        if (session == null)
        {
            _logger.LogWarning("Chat session not found: {SessionId}", sessionId);
            return null;
        }

        return _mapper.Map<ChatHistoryDto>(session);
    }

    private async Task<ChatSessionEntity> GetOrCreateSessionAsync(
        string userFingerprint,
        Guid? sessionId,
        CancellationToken cancellationToken)
    {
        ChatSessionEntity? session = null;

        // Try to get existing session by ID
        if (sessionId.HasValue)
        {
            session = await _chatRepository.GetSessionByIdAsync(sessionId.Value, cancellationToken);
        }

        // If not found, try to get by fingerprint
        if (session == null)
        {
            session = await _chatRepository.GetSessionByFingerprintAsync(userFingerprint, cancellationToken);
        }

        // Create new session if none exists
        if (session == null)
        {
            _logger.LogInformation("Creating new chat session for fingerprint: {Fingerprint}", userFingerprint);
            session = new ChatSessionEntity
            {
                Id = Guid.NewGuid(),
                UserFingerprint = userFingerprint,
                CreatedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                IsActive = true
            };
            session = await _chatRepository.AddAsync(session, cancellationToken);
        }

        return session;
    }

    private List<ChatMessage> BuildChatMessages(List<ChatMessageEntity> conversationHistory)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(
                "You are a helpful AI assistant for an e-commerce platform. " +
                "You help customers with product inquiries, stock information, and product recommendations. " +
                "Be friendly, concise, and helpful. " +
                "When you need product information, use the available tools to query the database. " +
                "Always provide accurate information based on the data you receive.")
        };

        // Add conversation history
        foreach (var msg in conversationHistory)
        {
            if (msg.Role == "user")
            {
                messages.Add(new UserChatMessage(msg.Content));
            }
            else if (msg.Role == "assistant")
            {
                messages.Add(new AssistantChatMessage(msg.Content));
            }
        }

        return messages;
    }

    private List<ChatTool> GetChatTools()
    {
        var tools = new List<ChatTool>();

        // Tool 1: Search products
        tools.Add(ChatTool.CreateFunctionTool(
            functionName: "search_products",
            functionDescription: "Search for products in the catalog. Use this when user asks about products, stock, prices, or wants recommendations.",
            functionParameters: BinaryData.FromString("""
            {
                "type": "object",
                "properties": {
                    "query": {
                        "type": "string",
                        "description": "Search query for product name or description"
                    },
                    "category": {
                        "type": "string",
                        "description": "Filter by product category (optional)"
                    },
                    "maxResults": {
                        "type": "integer",
                        "description": "Maximum number of results to return (default: 10, max: 50)"
                    }
                },
                "required": ["query"]
            }
            """)));

        // Tool 2: Get product by SKU
        tools.Add(ChatTool.CreateFunctionTool(
            functionName: "get_product_by_sku",
            functionDescription: "Get detailed information about a specific product by its SKU code.",
            functionParameters: BinaryData.FromString("""
            {
                "type": "object",
                "properties": {
                    "sku": {
                        "type": "string",
                        "description": "The SKU code of the product"
                    }
                },
                "required": ["sku"]
            }
            """)));

        return tools;
    }

    private async Task<string> ExecuteToolAsync(ChatToolCall toolCall, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing tool: {ToolName}", toolCall.FunctionName);

        try
        {
            var functionArgs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(toolCall.FunctionArguments.ToString());

            switch (toolCall.FunctionName)
            {
                case "search_products":
                    return await SearchProductsToolAsync(functionArgs, cancellationToken);

                case "get_product_by_sku":
                    return await GetProductBySkuToolAsync(functionArgs, cancellationToken);

                default:
                    _logger.LogWarning("Unknown tool requested: {ToolName}", toolCall.FunctionName);
                    return JsonSerializer.Serialize(new { error = "Unknown tool" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool: {ToolName}", toolCall.FunctionName);
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private async Task<string> SearchProductsToolAsync(Dictionary<string, JsonElement>? args, CancellationToken cancellationToken)
    {
        if (args == null || !args.ContainsKey("query"))
        {
            return JsonSerializer.Serialize(new { error = "Missing required parameter: query" });
        }

        var query = args["query"].GetString() ?? "";
        var category = args.ContainsKey("category") ? args["category"].GetString() : null;
        var maxResults = args.ContainsKey("maxResults") ? args["maxResults"].GetInt32() : 10;

        // Limit max results to 50
        maxResults = Math.Min(maxResults, 50);

        _logger.LogInformation("Searching products: Query={Query}, Category={Category}, MaxResults={MaxResults}",
            query, category, maxResults);

        var searchRequest = new ProductSearchRequest
        {
            Name = query,
            Description = query,
            Category = category,
            PageNumber = 1,
            PageSize = maxResults
        };

        var products = await _productRepository.SearchForAIToolAsync(searchRequest, cancellationToken);

        var result = new
        {
            totalCount = products.TotalCount,
            products = products.Items.Select(p => new
            {
                name = p.Name,
                description = p.Description,
                sku = p.SKU,
                price = p.Price,
                stockQuantity = p.StockQuantity,
                category = p.Category,
                isActive = p.IsActive
            }).ToList()
        };

        return JsonSerializer.Serialize(result);
    }

    private async Task<string> GetProductBySkuToolAsync(Dictionary<string, JsonElement>? args, CancellationToken cancellationToken)
    {
        if (args == null || !args.ContainsKey("sku"))
        {
            return JsonSerializer.Serialize(new { error = "Missing required parameter: sku" });
        }

        var sku = args["sku"].GetString() ?? "";

        _logger.LogInformation("Getting product by SKU: {SKU}", sku);

        var product = await _productRepository.GetBySkuAsync(sku, cancellationToken);

        if (product == null)
        {
            return JsonSerializer.Serialize(new { error = $"Product not found with SKU: {sku}" });
        }

        var result = new
        {
            name = product.Name,
            description = product.Description,
            sku = product.SKU,
            price = product.Price,
            stockQuantity = product.StockQuantity,
            category = product.Category,
            isActive = product.IsActive
        };

        return JsonSerializer.Serialize(result);
    }
}
