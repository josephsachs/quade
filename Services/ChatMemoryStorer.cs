using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quade.Models;

namespace Quade.Services;

public class ChatMemoryStorer
{
    private readonly ModelProviderResolver _providerResolver;
    private readonly ThoughtProcessLogger _logger;
    private readonly ConfigService _configService;

    private const int MEMORY_STORE_INTERVAL = 16;

    public ChatMemoryStorer(
        ModelProviderResolver providerResolver,
        ThoughtProcessLogger logger,
        ConfigService configService)
    {
        _providerResolver = providerResolver;
        _logger = logger;
        _configService = configService;
    }

    public async Task<bool> ProcessMemories(List<Message> allMessages)
    {
        var unmemoizedMessages = GetUnmemoizedMessages(allMessages);
        
        if (!ShouldProcessMemories(unmemoizedMessages))
            return false;

        _logger.LogInfo($"Processing {unmemoizedMessages.Count()} messages into memory...");

        var transcript = FormatAsTranscript(unmemoizedMessages);
        var summary = await GenerateMemorySummary(transcript);
        var paragraphs = ExtractParagraphs(summary);
        
        if (paragraphs.Count == 0)
        {
            MarkMessagesAsMemorized(allMessages);
            return false;
        }

        // TODO: Embed and store paragraphs

        MarkMessagesAsMemorized(allMessages);
        return true;
    }

    private List<Message> GetUnmemoizedMessages(List<Message> allMessages)
    {
        return allMessages.Where(m => !m.IsMemorized).ToList();
    }

    private bool ShouldProcessMemories(List<Message> unmemoizedMessages)
    {
        return unmemoizedMessages.Count >= MEMORY_STORE_INTERVAL;
    }

    private string FormatAsTranscript(List<Message> messages)
    {
        return string.Join("\n\n", messages.Select(m =>
        {
            var participant = m.IsUser ? "User" : "Assistant";
            return $"{participant}: \"{m.Content}\"";
        }));
    }

    private async Task<string> GenerateMemorySummary(string transcript)
    {
        var config = await _configService.LoadConfigAsync();
        var provider = _providerResolver.GetProviderForModel(config.MemoryModel);

        var requestConfig = new ModelRequestConfig
        {
            Model = config.MemoryModel,
            MaxTokens = 500
        };

        var prompt = BuildMemoryPrompt(transcript);
        var promptMessages = new List<Message>
        {
            new Message
            {
                Content = prompt,
                IsUser = true
            }
        };

        var response = await provider.SendMessageAsync(requestConfig, promptMessages);

        _logger.LogInfo($"{response}");
        
        return response;
    }

    private string BuildMemoryPrompt(string transcript)
    {
        return $$"""
            Here is a conversation transcript:

            {{transcript}}

            Review this conversation and extract only what's worth remembering - knowledge about the user, idiosyncratic definitions, significant insights, or notable events. Skip routine pleasantries and generic exchanges.

            Write one paragraph per distinct topic, with as much or as little detail as that topic warrants. Separate paragraphs with a blank line.

            Do not use headings, bullet points, numbered lists, bold text, or any other formatting. Do not prefix paragraphs with labels like 'Topic:' or 'Memory:'. Just plain prose.
            """;
    }

    private List<string> ExtractParagraphs(string summary)
    {
        // Sanity check for formatting violations
        if (summary.Contains("**") || summary.Contains("- ") || summary.Contains("1."))
        {
            _logger.LogInfo("Warning: Memory summary contains formatting markers");
        }

        var paragraphs = summary
            .Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .ToList();
        
        return paragraphs;
    }

    private void MarkMessagesAsMemorized(List<Message> allMessages)
    {
        var count = allMessages.Count(m => !m.IsMemorized);
        
        allMessages
            .Where(m => !m.IsMemorized)
            .ToList()
            .ForEach(m => m.IsMemorized = true);
    }
}