using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Quade.Models;

namespace Quade.Services;

public class ConfigService
{
    private readonly string _configDir;
    private readonly string _configPath;

    public ConfigService()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _configDir = Path.Combine(home, ".quade");
        _configPath = Path.Combine(_configDir, "config.json");
    }

    public async Task<AppConfig> LoadConfigAsync()
    {
        if (!File.Exists(_configPath))
        {
            return new AppConfig();
        }

        var json = await File.ReadAllTextAsync(_configPath);
        return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
    }

    public async Task SaveConfigAsync(AppConfig config)
    {
        Directory.CreateDirectory(_configDir);

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        await File.WriteAllTextAsync(_configPath, json);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || 
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            File.SetUnixFileMode(_configPath, 
                UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    public async Task<bool> HasApiKeyAsync()
    {
        var config = await LoadConfigAsync();
        return !string.IsNullOrWhiteSpace(config.ApiKey);
    }
}