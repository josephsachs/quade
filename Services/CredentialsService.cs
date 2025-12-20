using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Quade.Services;

public class CredentialsService
{
    public const string ANTHROPIC = "anthropic";
    public const string OPENAI = "openai";
    public const string ANLATAN = "anlatan";

    private readonly string _credentialsPath;
    private readonly byte[] _encryptionKey;

    private const int KEY_SIZE = 32;
    private const int IV_SIZE = 16;
    private const int PBKDF2_ITERATIONS = 100000;

    public CredentialsService()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var configDir = Path.Combine(home, ".quade");
        _credentialsPath = Path.Combine(configDir, "credentials.enc");
        
        Directory.CreateDirectory(configDir);

        var machineId = $"{Environment.UserName}@{Environment.MachineName}";
        var salt = Encoding.UTF8.GetBytes("quade.credentials.v1");
        
        using var pbkdf2 = new Rfc2898DeriveBytes(
            machineId,
            salt,
            PBKDF2_ITERATIONS,
            HashAlgorithmName.SHA256);
        
        _encryptionKey = pbkdf2.GetBytes(KEY_SIZE);
    }

    public async Task<bool> HasApiKeyAsync(string provider)
    {
        var key = await GetApiKeyAsync(provider);
        return !string.IsNullOrWhiteSpace(key);
    }

    public async Task<string?> GetApiKeyAsync(string provider)
    {
        if (!File.Exists(_credentialsPath))
            return null;

        var credentials = await LoadCredentialsAsync();
        
        if (!credentials.TryGetValue(provider, out var encryptedValue))
            return null;

        return DecryptValue(encryptedValue);
    }

    public async Task SetApiKeyAsync(string provider, string apiKey)
    {
        var credentials = await LoadCredentialsAsync();
        
        var encryptedValue = EncryptValue(apiKey);
        credentials[provider] = encryptedValue;
        
        await SaveCredentialsAsync(credentials);
    }

    public async Task DeleteApiKeyAsync(string provider)
    {
        var credentials = await LoadCredentialsAsync();
        
        credentials.Remove(provider);
        
        await SaveCredentialsAsync(credentials);
    }

    private async Task<Dictionary<string, string>> LoadCredentialsAsync()
    {
        if (!File.Exists(_credentialsPath))
            return new Dictionary<string, string>();

        var json = await File.ReadAllTextAsync(_credentialsPath);
        
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) 
            ?? new Dictionary<string, string>();
    }

    private async Task SaveCredentialsAsync(Dictionary<string, string> credentials)
    {
        var json = JsonSerializer.Serialize(credentials, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        await File.WriteAllTextAsync(_credentialsPath, json);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || 
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            File.SetUnixFileMode(_credentialsPath, 
                UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    private string EncryptValue(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipherBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

        var combined = new byte[IV_SIZE + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, combined, 0, IV_SIZE);
        Buffer.BlockCopy(cipherBytes, 0, combined, IV_SIZE, cipherBytes.Length);

        return Convert.ToBase64String(combined);
    }

    private string DecryptValue(string encryptedValue)
    {
        var combined = Convert.FromBase64String(encryptedValue);

        var iv = new byte[IV_SIZE];
        var cipherBytes = new byte[combined.Length - IV_SIZE];
        
        Buffer.BlockCopy(combined, 0, iv, 0, IV_SIZE);
        Buffer.BlockCopy(combined, IV_SIZE, cipherBytes, 0, cipherBytes.Length);

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plaintextBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plaintextBytes);
    }
}