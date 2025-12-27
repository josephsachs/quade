using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quade.Models;

namespace Quade.Services;

public class ChatContextBuilder
{
    private const int MAX_CONTEXT_MESSAGES = 16;
    private const int TOP_K_MEMORIES = 5;

    private readonly VectorProviderResolver _vectorProviderResolver;
    private readonly VectorStorageResolver _vectorStorageResolver;
    private readonly ConfigService _configService;
    private readonly ThoughtProcessLogger _logger;

    public ChatContextBuilder(
        VectorProviderResolver vectorProviderResolver,
        VectorStorageResolver vectorStorageResolver,
        ConfigService configService,
        ThoughtProcessLogger logger)
    {
        _vectorProviderResolver = vectorProviderResolver;
        _vectorStorageResolver = vectorStorageResolver;
        _configService = configService;
        _logger = logger;
    }

    public List<Message> BuildContext(List<Message> allMessages)
    {
        if (allMessages.Count <= MAX_CONTEXT_MESSAGES)
        {
            return allMessages;
        }

        return allMessages.TakeLast(MAX_CONTEXT_MESSAGES).ToList();
    }

    public async Task<string> AugmentSystemPromptWithMemories(string baseSystemPrompt, string userMessage)
    {
        var config = await _configService.LoadConfigAsync();

        if (string.IsNullOrEmpty(config.VectorModel) || 
            string.IsNullOrEmpty(config.MemoryModel))
        {
            _logger.LogInfo("Memory system not configured, skipping memory retrieval");
            return baseSystemPrompt;
        }

        try
        {
            var vectorProvider = _vectorProviderResolver.GetProviderForModel(config.VectorModel);
            var embedding = await vectorProvider.GetEmbeddingAsync(userMessage);

            var storage = _vectorStorageResolver.GetStorage(config.SelectedVectorStorage);
            var memories = await storage.SearchSimilarMemoriesAsync(embedding, TOP_K_MEMORIES, threshold: 0.0f);

            if (memories.Count == 0)
            {
                _logger.LogInfo("No memories found for current query");
                return baseSystemPrompt;
            }

            _logger.LogInfo($"Retrieved {memories.Count} memories:");
            foreach (var memory in memories)
            {
                _logger.LogInfo($"  - [{memory.Similarity:F3}] {TruncateForLog(memory.Content)}");
            }

            var memoryBlock = FormatMemoriesForPrompt(memories);
            return $"{baseSystemPrompt}\n\n{memoryBlock}\n\nUse these memories to inform your response when relevant.";
        }
        catch (System.Exception ex)
        {
            _logger.LogInfo($"Memory retrieval failed: {ex.Message}");
            return baseSystemPrompt;
        }
    }

    private string FormatMemoriesForPrompt(List<Memory> memories)
    {
        var lines = new List<string> { "Relevant memories from past conversations:" };
        
        foreach (var memory in memories)
        {
            lines.Add($"- {memory.Content}");
        }

        return string.Join("\n", lines);
    }

    private string TruncateForLog(string content, int maxLength = 80)
    {
        if (content.Length <= maxLength)
            return content;
        
        return content.Substring(0, maxLength) + "...";
    }
}