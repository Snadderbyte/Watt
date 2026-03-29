using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Watt.Core.Authentication;

/// <summary>
/// Manages secure storage and retrieval of Dataverse credentials.
/// Stores credentials in an encrypted format in the user's local app data.
/// </summary>
public class CredentialManager : IAsyncDisposable
{
    private const string CredentialsFileName = "watt_credentials.json";
    private const string EnvironmentsFileName = "watt_environments.json";
    private readonly string _storageDirectory;
    private readonly Dictionary<string, Credentials> _credentialsCache;
    private readonly Dictionary<string, EnvironmentDetails> _environmentsCache;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public CredentialManager()
    {
        _storageDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Watt"
        );

        _credentialsCache = new Dictionary<string, Credentials>(StringComparer.OrdinalIgnoreCase);
        _environmentsCache = new Dictionary<string, EnvironmentDetails>(StringComparer.OrdinalIgnoreCase);

        Directory.CreateDirectory(_storageDirectory);
    }

    /// <summary>
    /// Saves credentials securely for an environment.
    /// </summary>
    public async Task SaveCredentialsAsync(Credentials credentials)
    {
        _credentialsCache[credentials.EnvironmentId] = credentials;
        await PersistCredentialsAsync();
    }

    /// <summary>
    /// Retrieves cached credentials for an environment, or null if not found.
    /// </summary>
    public Credentials? GetCredentials(string environmentId)
    {
        if (_credentialsCache.TryGetValue(environmentId, out var creds))
        {
            return creds;
        }
        return null;
    }

    /// <summary>
    /// Loads all stored credentials from disk into cache.
    /// </summary>
    public async Task LoadStoredCredentialsAsync()
    {
        var credentialsFile = Path.Combine(_storageDirectory, CredentialsFileName);
        if (!File.Exists(credentialsFile))
            return;

        try
        {
            var json = await File.ReadAllTextAsync(credentialsFile);
            var decrypted = DecryptData(json);
            var credentialsData = JsonSerializer.Deserialize<Dictionary<string, StoredCredentials>>(decrypted, JsonOptions);

            if (credentialsData != null)
            {
                _credentialsCache.Clear();
                foreach (var kvp in credentialsData)
                {
                    _credentialsCache[kvp.Key] = kvp.Value.ToCredentials();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading credentials: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves environment configuration.
    /// </summary>
    public async Task SaveEnvironmentAsync(EnvironmentDetails environment)
    {
        _environmentsCache[environment.Id] = environment;
        await PersistEnvironmentsAsync();
    }

    /// <summary>
    /// Retrieves an environment configuration by ID.
    /// </summary>
    public EnvironmentDetails? GetEnvironment(string id)
    {
        if (_environmentsCache.TryGetValue(id, out var env))
        {
            return env;
        }
        return null;
    }

    /// <summary>
    /// Gets all registered environments.
    /// </summary>
    public IEnumerable<EnvironmentDetails> GetAllEnvironments() => _environmentsCache.Values;

    /// <summary>
    /// Loads all stored environments from disk into cache.
    /// </summary>
    public async Task LoadStoredEnvironmentsAsync()
    {
        var environmentsFile = Path.Combine(_storageDirectory, EnvironmentsFileName);
        if (!File.Exists(environmentsFile))
            return;

        try
        {
            var json = await File.ReadAllTextAsync(environmentsFile);
            var environments = JsonSerializer.Deserialize<Dictionary<string, EnvironmentDetails>>(json, JsonOptions);

            if (environments != null)
            {
                _environmentsCache.Clear();
                foreach (var kvp in environments)
                {
                    _environmentsCache[kvp.Key] = kvp.Value;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading environments: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes credentials for an environment.
    /// </summary>
    public async Task DeleteCredentialsAsync(string environmentId)
    {
        _credentialsCache.Remove(environmentId);
        await PersistCredentialsAsync();
    }

    /// <summary>
    /// Deletes an environment configuration.
    /// </summary>
    public async Task DeleteEnvironmentAsync(string environmentId)
    {
        _environmentsCache.Remove(environmentId);
        _credentialsCache.Remove(environmentId);
        await PersistCredentialsAsync();
        await PersistEnvironmentsAsync();
    }

    private async Task PersistCredentialsAsync()
    {
        try
        {
            var toStore = new Dictionary<string, StoredCredentials>();
            foreach (var kvp in _credentialsCache)
            {
                toStore[kvp.Key] = StoredCredentials.FromCredentials(kvp.Value);
            }

            var json = JsonSerializer.Serialize(toStore, JsonOptions);
            var encrypted = EncryptData(json);

            var credentialsFile = Path.Combine(_storageDirectory, CredentialsFileName);
            await File.WriteAllTextAsync(credentialsFile, encrypted);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error persisting credentials: {ex.Message}");
            throw;
        }
    }

    private async Task PersistEnvironmentsAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_environmentsCache, JsonOptions);
            var environmentsFile = Path.Combine(_storageDirectory, EnvironmentsFileName);
            await File.WriteAllTextAsync(environmentsFile, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error persisting environments: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Encrypts data using DPAPI (Windows Data Protection API) for user-specific encryption.
    /// </summary>
    private string EncryptData(string data)
    {
        var dataBytes = System.Text.Encoding.UTF8.GetBytes(data);
        var encryptedBytes = ProtectedData.Protect(dataBytes, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// Decrypts data that was encrypted with DPAPI.
    /// </summary>
    private string DecryptData(string encryptedData)
    {
        var encryptedBytes = Convert.FromBase64String(encryptedData);
        var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
        return System.Text.Encoding.UTF8.GetString(decryptedBytes);
    }

    public async ValueTask DisposeAsync()
    {
        _credentialsCache.Clear();
        _environmentsCache.Clear();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Internal class for JSON serialization of credentials.
    /// </summary>
    private class StoredCredentials
    {
        public AuthenticationMethod Method { get; set; }
        public string? EnvironmentId { get; set; }
        public DateTime CreatedAt { get; set; }

        // OAuth
        public string? AccessToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? RefreshToken { get; set; }

        // ClientSecret
        public string? TenantId { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }

        // UsernamePassword
        public string? Username { get; set; }
        public string? Password { get; set; }

        public static StoredCredentials FromCredentials(Credentials creds) => creds switch
        {
            OAuthCredentials oauth => new()
            {
                Method = AuthenticationMethod.OAuth,
                EnvironmentId = oauth.EnvironmentId,
                CreatedAt = oauth.CreatedAt,
                AccessToken = oauth.AccessToken,
                ExpiresAt = oauth.ExpiresAt,
                RefreshToken = oauth.RefreshToken
            },
            ClientSecretCredentials clientSecret => new()
            {
                Method = AuthenticationMethod.ClientSecret,
                EnvironmentId = clientSecret.EnvironmentId,
                CreatedAt = clientSecret.CreatedAt,
                TenantId = clientSecret.TenantId,
                ClientId = clientSecret.ClientId,
                ClientSecret = clientSecret.ClientSecret
            },
            UsernamePasswordCredentials userPass => new()
            {
                Method = AuthenticationMethod.UsernamePassword,
                EnvironmentId = userPass.EnvironmentId,
                CreatedAt = userPass.CreatedAt,
                Username = userPass.Username,
                Password = userPass.Password
            },
            _ => throw new NotSupportedException($"Credentials type {creds.GetType()} not supported")
        };

        public Credentials ToCredentials() => Method switch
        {
            AuthenticationMethod.OAuth => new OAuthCredentials
            {
                EnvironmentId = EnvironmentId!,
                CreatedAt = CreatedAt,
                AccessToken = AccessToken!,
                ExpiresAt = ExpiresAt ?? DateTime.UtcNow,
                RefreshToken = RefreshToken
            },
            AuthenticationMethod.ClientSecret => new ClientSecretCredentials
            {
                EnvironmentId = EnvironmentId!,
                CreatedAt = CreatedAt,
                TenantId = TenantId!,
                ClientId = ClientId!,
                ClientSecret = ClientSecret!
            },
            AuthenticationMethod.UsernamePassword => new UsernamePasswordCredentials
            {
                EnvironmentId = EnvironmentId!,
                CreatedAt = CreatedAt,
                Username = Username!,
                Password = Password!
            },
            _ => throw new NotSupportedException($"Authentication method {Method} not supported")
        };
    }
}
