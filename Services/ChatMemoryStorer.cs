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

    var messages = allMessages.Where(message => !message.IsMemorized);

    //if (messages.Count() < MEMORY_STORE_INTERVAL)
    //{
    //  return false;
    //}

    _logger.LogInfo($"Processing messages for memory, found {messages.Count()} messages.");
    _logger.LogInfo($"Identifying salient informatioon...");

    var config = await _configService.LoadConfigAsync();
    var provider = _providerResolver.GetProviderForModel(config.MemoryModel);

    var requestConfig = new ModelRequestConfig
    {
      Model = config.MemoryModel,
      MaxTokens = 500
    };

    var response = await provider.SendMessageAsync(
      requestConfig,
      messages.ToList()//,
      //$"Generate a summary of the messages, collecting knowledge about the user or described events, idiosyncratic definitions by either user or Claude, significant topics in the text, and insights shared or arrived at in conversation. Place each in a separate paragraph."
    );

    allMessages
      .Where(m => !m.IsMemorized).ToList()
      .ForEach(m => m.IsMemorized = true);

    return false;
  }
}