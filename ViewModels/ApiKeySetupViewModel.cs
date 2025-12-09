using System;
using System.Threading.Tasks;
using ReactiveUI;
using Quade.Models;
using Quade.Services;

namespace Quade.ViewModels;

public class ApiKeySetupViewModel : ViewModelBase
{
    private readonly ConfigService _configService;
    private readonly ApiClient _apiClient;
    
    private string _apiKey = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isValidating;

    public string ApiKey
    {
        get => _apiKey;
        set => this.RaiseAndSetIfChanged(ref _apiKey, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public bool IsValidating
    {
        get => _isValidating;
        set => this.RaiseAndSetIfChanged(ref _isValidating, value);
    }

    public ApiKeySetupViewModel(ConfigService configService, ApiClient apiClient)
    {
        _configService = configService;
        _apiClient = apiClient;
    }

    public async Task<bool> ValidateAndSaveAsync()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            ErrorMessage = "API key cannot be empty";
            return false;
        }

        IsValidating = true;
        ErrorMessage = string.Empty;

        try
        {
            _apiClient.SetApiKey(ApiKey);
            await _apiClient.GetAvailableModelsAsync();

            var config = await _configService.LoadConfigAsync();
            config.ApiKey = ApiKey;
            await _configService.SaveConfigAsync(config);

            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Invalid API key: {ex.Message}";
            return false;
        }
        finally
        {
            IsValidating = false;
        }
    }
}