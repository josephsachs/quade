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

  private const int MEMORY_STORE_INTERVAL = 24;

  public ChatMemoryStorer(
      ModelProviderResolver providerResolver,
      ThoughtProcessLogger logger,
      ConfigService configService)
  {
    _providerResolver = providerResolver;
    _logger = logger;
    _configService = configService;
  }

  public async Task<bool> GetMemoryEntries(List<Message> allMessages)
  {
    _logger.LogInfo($"Checking for memories to store...");

    var unmemoizedMessages = allMessages.Where(message => !message.IsMemorized).ToList();

    _logger.LogInfo($"Processing messages for memory, found {unmemoizedMessages.Count()} messages.");
    _logger.LogInfo($"Identifying salient information...");

    // Format the conversation as a transcript
    var transcript = string.Join("\n\n", unmemoizedMessages.Select(m =>
    {
      var participant = m.IsUser ? "User" : "Assistant";
      return $"{participant}: \"{m.Content}\"";
    }));

    var config = await _configService.LoadConfigAsync();
    var provider = _providerResolver.GetProviderForModel(config.MemoryModel);

    var requestConfig = new ModelRequestConfig
    {
      Model = config.MemoryModel,
      MaxTokens = 500
    };

    // Create a single-message list with the formatted transcript
    var promptMessages = new List<Message>
    {
        new Message
        {
            Content = $"Here is a conversation transcript:\n\n{transcript}\n\nGenerate a summary of this conversation, collecting knowledge about the user or described events, idiosyncratic definitions by either user or Assistant, significant topics in the text, and insights shared or arrived at in conversation. Place each in a separate paragraph.",
            IsUser = true
        }
    };

    var response = await provider.SendMessageAsync(
        requestConfig,
        promptMessages
    );

    allMessages
        .Where(m => !m.IsMemorized).ToList()
        .ForEach(m => m.IsMemorized = true);

    _logger.LogInfo($"Memory summary generated: {response}");

    return true;
  }
}