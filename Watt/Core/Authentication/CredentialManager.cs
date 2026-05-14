using System.Text.Json;

namespace Watt.Core.Authentication;

/// <summary>
/// Manages persistent storage of Dataverse environment configurations.
/// No credentials are stored — authentication is delegated to the Azure CLI.
/// </summary>
public class CredentialManager : IAsyncDisposable
{
    private const string EnvironmentsFileName = "watt_environments.json";
    /// <summary>
    /// The directory where environment configurations are stored. This is typically %APPDATA%\Watt on Windows.
    /// </summary>
    private readonly string _storageDirectory;
    /// <summary>
    /// In-memory cache of environment details, keyed by environment ID. This is loaded from disk on startup and updated as environments are added/removed.
    /// </summary>
    private readonly Dictionary<string, EnvironmentDetails> _environmentsCache;
    /// <summary>
    /// JSON serialization options for storing environment details. Uses camelCase naming and is case-insensitive on deserialization.
    /// </summary>
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

    public EnvironmentDetails? GetActiveEnvironment()
    {
        foreach (var env in _environmentsCache.Values)
        {
            if (env.IsActive)
                return env;
        }
        return null;
    }

    public async Task SetActiveEnvironmentAsync(string id)
    {
        foreach (var env in _environmentsCache.Values)
            env.IsActive = env.Id.Equals(id, StringComparison.OrdinalIgnoreCase);

        await PersistEnvironmentsAsync();
    }

    public IEnumerable<EnvironmentDetails> GetAllEnvironments() => _environmentsCache.Values;

    /// <summary>
    /// Loads stored environment configurations from disk into the in-memory cache. This does not load any credentials, as they are not stored by Watt.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
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

    /// <summary>
    /// Persists the current in-memory cache of environment configurations to disk as a JSON file. This overwrites the existing file with the current state of the cache. This does not persist any credentials, as they are not stored by Watt.
    /// </summary>
    /// <returns></returns>
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
    /// Disposes of the CredentialManager by clearing the in-memory cache. This does not delete any credentials, as they are not stored by Watt. Since there are no unmanaged resources, this simply clears the cache and completes immediately.
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        _environmentsCache.Clear();
        await Task.CompletedTask;
    }
}
