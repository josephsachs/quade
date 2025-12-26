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

    public async Task<bool> StoreIfInterval(List<Message> allMessages)
    {
        var messages = allMessages.Where(message => !message.IsMemorized);

        _logger.LogInfo($"Processing messages for memory, found {messages.Count()} messages");
        _logger.LogInfo($"Identifying salient informatioon");

        var config = await _configService.LoadConfigAsync();
        var provider = _providerResolver.GetProviderForModel(config.ThoughtModel);

        var requestConfig = new ModelRequestConfig
        {
          Model = config.ThoughtModel
        };

        var response = await provider.SendMessageAsync(
                requestConfig,
                messages.ToList(),
                $"Identify facts about the user, relevant definitions or insights in the following text and produce a list of summaries separated by line breaks."
        );

        return false;
    }
}