using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Watt.Core.Authentication;

/// <summary>
/// Manages persistent storage of Dataverse environment configurations.
/// No credentials are stored — authentication is delegated to the Azure CLI.
/// </summary>
public class CredentialManager : IAsyncDisposable
{
    private const string EnvironmentsFileName = "watt_environments.json";
    private readonly string _storageDirectory;
    private readonly Dictionary<string, EnvironmentDetails> _environmentsCache;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public CredentialManager()
    {
        _storageDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Watt"
        );
        _environmentsCache = new Dictionary<string, EnvironmentDetails>(StringComparer.OrdinalIgnoreCase);
        Directory.CreateDirectory(_storageDirectory);
    }

    public async Task SaveEnvironmentAsync(EnvironmentDetails environment)
    {
        _environmentsCache[environment.Id] = environment;
        await PersistEnvironmentsAsync();
    }

    public EnvironmentDetails? GetEnvironment(string id)
    {
        _environmentsCache.TryGetValue(id, out var env);
        return env;
    }

    public IEnumerable<EnvironmentDetails> GetAllEnvironments() => _environmentsCache.Values;

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
                    _environmentsCache[kvp.Key] = kvp.Value;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading environments: {ex.Message}");
        }
    }

    public async Task DeleteEnvironmentAsync(string environmentId)
    {
        _environmentsCache.Remove(environmentId);
        await PersistEnvironmentsAsync();
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

    public async ValueTask DisposeAsync()
    {
        _environmentsCache.Clear();
        await Task.CompletedTask;
    }
}
