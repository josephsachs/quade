using System;
using System.Threading.Tasks;
using ReactiveUI;
using Quade.Services;

namespace Quade.ViewModels;

public class SettingsWindowViewModel : ViewModelBase
{
    private readonly CredentialsService _credentialsService;
    private readonly ApiClient _apiClient;

    private string _anthropicKeyDisplay = "(not set)";
    private string _openaiKeyDisplay = "(not set)";
    private string _anlatanKeyDisplay = "(not set)";
    private string _supabaseKeyDisplay = "(not set)";

    private bool _hasAnthropicKey;
    private bool _hasOpenaiKey;
    private bool _hasAnlatanKey;
    private bool _hasSupabaseKey;

    private string _anthropicKeyInput = string.Empty;
    private string _openaiKeyInput = string.Empty;
    private string _anlatanKeyInput = string.Empty;
    private string _supabaseKeyInput = string.Empty;

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

    public string SupabaseKeyDisplay {
        get => _supabaseKeyDisplay;
        set => this.RaiseAndSetIfChanged(ref _supabaseKeyDisplay, value);
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

    public SettingsWindowViewModel(CredentialsService credentialsService, ApiClient apiClient)
    {
        _credentialsService = credentialsService;
        _apiClient = apiClient;
    }

    public async Task LoadKeysAsync()
    {
        await UpdateKeyDisplayAsync(CredentialsService.ANTHROPIC);
        await UpdateKeyDisplayAsync(CredentialsService.OPENAI);
        await UpdateKeyDisplayAsync(CredentialsService.ANLATAN);
        await UpdateKeyDisplayAsync(CredentialsService.SUPABASE);
    }

    public async Task AddOrReplaceKeyAsync(string provider)
    {
        string keyInput = provider switch
        {
            CredentialsService.ANTHROPIC => AnthropicKeyInput,
            CredentialsService.OPENAI => OpenaiKeyInput,
            CredentialsService.ANLATAN => AnlatanKeyInput,
            CredentialsService.SUPABASE => SupabaseKeyInput,
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(keyInput))
            return;

        await _credentialsService.SetApiKeyAsync(provider, keyInput);

        if (provider == CredentialsService.ANTHROPIC)
        {
            _apiClient.SetApiKey(keyInput);
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
        }
    }
}