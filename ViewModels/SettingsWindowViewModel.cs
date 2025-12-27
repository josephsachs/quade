using System;
using System.Threading.Tasks;
using ReactiveUI;
using Quade.Services;
using Quade.Models;

namespace Quade.ViewModels;

public class SettingsWindowViewModel : ViewModelBase
{
    private readonly CredentialsService _credentialsService;
    private readonly AnthropicClient _anthropicClient;
    private readonly ConfigService _configService;

    private string _anthropicKeyDisplay = "(not set)";
    private string _openaiKeyDisplay = "(not set)";
    private string _anlatanKeyDisplay = "(not set)";
    private string _supabaseKeyDisplay = "(not set)";
    private string _qdrantKeyDisplay = "(not set)";

    private bool _hasAnthropicKey;
    private bool _hasOpenaiKey;
    private bool _hasAnlatanKey;
    private bool _hasSupabaseKey;
    private bool _hasQdrantKey;

    private string _anthropicKeyInput = string.Empty;
    private string _openaiKeyInput = string.Empty;
    private string _anlatanKeyInput = string.Empty;
    private string _supabaseKeyInput = string.Empty;
    private string _qdrantKeyInput = string.Empty;

    private string _supabaseUrlInput = string.Empty;
    private string _qdrantUrlInput = string.Empty;

    private bool _isSupabaseSelected;
    private bool _isQdrantSelected;

    public string AnthropicKeyDisplay
    {
        get => _anthropicKeyDisplay;
        set => this.RaiseAndSetIfChanged(ref _anthropicKeyDisplay, value);
    }

    public string OpenaiKeyDisplay
    {
        get => _openaiKeyDisplay;
        set => this.RaiseAndSetIfChanged(ref _openaiKeyDisplay, value);
    }

    public string AnlatanKeyDisplay
    {
        get => _anlatanKeyDisplay;
        set => this.RaiseAndSetIfChanged(ref _anlatanKeyDisplay, value);
    }

    public string SupabaseKeyDisplay
    {
        get => _supabaseKeyDisplay;
        set => this.RaiseAndSetIfChanged(ref _supabaseKeyDisplay, value);
    }

    public string QdrantKeyDisplay
    {
        get => _qdrantKeyDisplay;
        set => this.RaiseAndSetIfChanged(ref _qdrantKeyDisplay, value);
    }

    public bool HasAnthropicKey
    {
        get => _hasAnthropicKey;
        set => this.RaiseAndSetIfChanged(ref _hasAnthropicKey, value);
    }

    public bool HasOpenaiKey
    {
        get => _hasOpenaiKey;
        set => this.RaiseAndSetIfChanged(ref _hasOpenaiKey, value);
    }

    public bool HasAnlatanKey
    {
        get => _hasAnlatanKey;
        set => this.RaiseAndSetIfChanged(ref _hasAnlatanKey, value);
    }

    public bool HasSupabaseKey
    {
        get => _hasSupabaseKey;
        set => this.RaiseAndSetIfChanged(ref _hasSupabaseKey, value);
    }

    public bool HasQdrantKey
    {
        get => _hasQdrantKey;
        set => this.RaiseAndSetIfChanged(ref _hasQdrantKey, value);
    }

    public string AnthropicKeyInput
    {
        get => _anthropicKeyInput;
        set => this.RaiseAndSetIfChanged(ref _anthropicKeyInput, value);
    }

    public string OpenaiKeyInput
    {
        get => _openaiKeyInput;
        set => this.RaiseAndSetIfChanged(ref _openaiKeyInput, value);
    }

    public string AnlatanKeyInput
    {
        get => _anlatanKeyInput;
        set => this.RaiseAndSetIfChanged(ref _anlatanKeyInput, value);
    }

    public string SupabaseKeyInput
    {
        get => _supabaseKeyInput;
        set => this.RaiseAndSetIfChanged(ref _supabaseKeyInput, value);
    }

    public string QdrantKeyInput
    {
        get => _qdrantKeyInput;
        set => this.RaiseAndSetIfChanged(ref _qdrantKeyInput, value);
    }

    public string SupabaseUrlInput
    {
        get => _supabaseUrlInput;
        set => this.RaiseAndSetIfChanged(ref _supabaseUrlInput, value);
    }

    public string QdrantUrlInput
    {
        get => _qdrantUrlInput;
        set => this.RaiseAndSetIfChanged(ref _qdrantUrlInput, value);
    }

    public bool IsSupabaseSelected
    {
        get => _isSupabaseSelected;
        set => this.RaiseAndSetIfChanged(ref _isSupabaseSelected, value);
    }

    public bool IsQdrantSelected
    {
        get => _isQdrantSelected;
        set => this.RaiseAndSetIfChanged(ref _isQdrantSelected, value);
    }

    public SettingsWindowViewModel(CredentialsService credentialsService, AnthropicClient anthropicClient, ConfigService configService)
    {
        _credentialsService = credentialsService;
        _anthropicClient = anthropicClient;
        _configService = configService;
    }

    public async Task LoadKeysAsync()
    {
        await UpdateKeyDisplayAsync(CredentialsService.ANTHROPIC);
        await UpdateKeyDisplayAsync(CredentialsService.OPENAI);
        await UpdateKeyDisplayAsync(CredentialsService.ANLATAN);
        await UpdateKeyDisplayAsync(CredentialsService.SUPABASE);
        await UpdateKeyDisplayAsync(CredentialsService.QDRANT);
        await LoadUrlsAsync();
        await LoadStorageProviderAsync();
    }

    private async Task LoadUrlsAsync()
    {
        var config = await _configService.LoadConfigAsync();
        SupabaseUrlInput = config.SupabaseUrl;
        QdrantUrlInput = config.QdrantUrl;
    }

    private async Task LoadStorageProviderAsync()
    {
        var config = await _configService.LoadConfigAsync();
        IsSupabaseSelected = config.SelectedVectorStorage == VectorStorageProvider.Supabase;
        IsQdrantSelected = config.SelectedVectorStorage == VectorStorageProvider.Qdrant;
    }

    public async Task SelectStorageProviderAsync(VectorStorageProvider provider)
    {
        var config = await _configService.LoadConfigAsync();
        config.SelectedVectorStorage = provider;
        await _configService.SaveConfigAsync(config);

        IsSupabaseSelected = provider == VectorStorageProvider.Supabase;
        IsQdrantSelected = provider == VectorStorageProvider.Qdrant;
    }

    public async Task SaveSupabaseUrlAsync()
    {
        var config = await _configService.LoadConfigAsync();
        config.SupabaseUrl = SupabaseUrlInput.Trim();
        await _configService.SaveConfigAsync(config);
    }

    public async Task SaveQdrantUrlAsync()
    {
        var config = await _configService.LoadConfigAsync();
        config.QdrantUrl = QdrantUrlInput.Trim();
        await _configService.SaveConfigAsync(config);
    }

    public async Task AddOrReplaceKeyAsync(string provider)
    {
        string keyInput = provider switch
        {
            CredentialsService.ANTHROPIC => AnthropicKeyInput,
            CredentialsService.OPENAI => OpenaiKeyInput,
            CredentialsService.ANLATAN => AnlatanKeyInput,
            CredentialsService.SUPABASE => SupabaseKeyInput,
            CredentialsService.QDRANT => QdrantKeyInput,
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(keyInput))
            return;

        await _credentialsService.SetApiKeyAsync(provider, keyInput);

        if (provider == CredentialsService.ANTHROPIC)
        {
            _anthropicClient.SetApiKey(keyInput);
        }

        ClearInput(provider);
        await UpdateKeyDisplayAsync(provider);
    }

    public async Task DeleteKeyAsync(string provider)
    {
        await _credentialsService.DeleteApiKeyAsync(provider);
        await UpdateKeyDisplayAsync(provider);
    }

    private async Task UpdateKeyDisplayAsync(string provider)
    {
        var key = await _credentialsService.GetApiKeyAsync(provider);
        var hasKey = !string.IsNullOrWhiteSpace(key);
        var display = hasKey && key?.Length >= 4 ? $"****{key[^4..]}" : "(not set)";

        switch (provider)
        {
            case CredentialsService.ANTHROPIC:
                HasAnthropicKey = hasKey;
                AnthropicKeyDisplay = display;
                break;
            case CredentialsService.OPENAI:
                HasOpenaiKey = hasKey;
                OpenaiKeyDisplay = display;
                break;
            case CredentialsService.ANLATAN:
                HasAnlatanKey = hasKey;
                AnlatanKeyDisplay = display;
                break;
            case CredentialsService.SUPABASE:
                HasSupabaseKey = hasKey;
                SupabaseKeyDisplay = display;
                break;
            case CredentialsService.QDRANT:
                HasQdrantKey = hasKey;
                QdrantKeyDisplay = display;
                break;
        }
    }

    private void ClearInput(string provider)
    {
        switch (provider)
        {
            case CredentialsService.ANTHROPIC:
                AnthropicKeyInput = string.Empty;
                break;
            case CredentialsService.OPENAI:
                OpenaiKeyInput = string.Empty;
                break;
            case CredentialsService.ANLATAN:
                AnlatanKeyInput = string.Empty;
                break;
            case CredentialsService.SUPABASE:
                SupabaseKeyInput = string.Empty;
                break;
            case CredentialsService.QDRANT:
                QdrantKeyInput = string.Empty;
                break;
        }
    }
}