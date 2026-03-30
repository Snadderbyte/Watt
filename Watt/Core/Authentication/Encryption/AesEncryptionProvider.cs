using System;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace Watt.Core.Authentication.Encryption;

/// <summary>
/// Cross-platform encryption provider using AES encryption.
/// Uses a master key derived from machine-specific and user-specific identifiers.
/// This provides adequate encryption for local credential storage across Windows, macOS, and Linux.
/// </summary>
public class AesEncryptionProvider : ICryptoProvider
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    /// <summary>
    /// Initializes a new instance of the AesEncryptionProvider.
    /// The encryption key is derived from machine and user identifiers to provide per-user,
    /// per-machine encryption similar to DPAPI but cross-platform.
    /// </summary>
    public AesEncryptionProvider()
    {
        // Derive encryption key from machine/user specific data
        // This ensures credentials are not transferable between machines or users
        var machineId = GetMachineIdentifier();
        var userId = Environment.UserName;
        
        // Use PBKDF2 to derive a strong key from machine and user identifiers
        var salt = Encoding.UTF8.GetBytes("Watt.Credentials.Salt");
        var keyMaterial = $"{machineId}:{userId}";
        var keyMaterialBytes = Encoding.UTF8.GetBytes(keyMaterial);
        
        // Use the static PBKDF2 method to derive key and IV
        _key = Rfc2898DeriveBytes.Pbkdf2(keyMaterialBytes, salt, iterations: 10000, HashAlgorithmName.SHA256, outputLength: 32);
        _iv = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes($"{keyMaterial}:IV"), salt, iterations: 10000, HashAlgorithmName.SHA256, outputLength: 16);
    }

    /// <summary>
    /// Encrypts plaintext to a base64-encoded ciphertext.
    /// </summary>
    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return string.Empty;

        try
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plaintext);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
        catch (Exception ex)
        {
            throw new CryptoException("Failed to encrypt data", ex);
        }
    }

    /// <summary>
    /// Decrypts base64-encoded ciphertext back to plaintext.
    /// </summary>
    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            return string.Empty;

        try
        {
            var buffer = Convert.FromBase64String(ciphertext);

            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream(buffer))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        catch (Exception ex)
        {
            throw new CryptoException("Failed to decrypt data", ex);
        }
    }

    /// <summary>
    /// Gets a machine-specific identifier for key derivation.
    /// This provides per-machine encryption similar to DPAPI.
    /// </summary>
    private static string GetMachineIdentifier()
    {
        try
        {
            // Try to get a machine-specific identifier
            if (OperatingSystem.IsWindows())
            {
                return GetWindowsMachineId();
            } 
            else if (OperatingSystem.IsMacOS())
            {
                return GetMacMachineId();
            }
            else if (OperatingSystem.IsLinux())
            {
                return GetLinuxMachineId();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Could not get machine identifier: {ex.Message}");
        }

        // Fallback to hostname if machine ID retrieval fails
        try
        {
            return System.Net.Dns.GetHostName();
        }
        catch
        {
            // Final fallback - use a default value
            // This means encryption will be user-specific but not machine-specific
            return "unknown-machine";
        }
    }

    /// <summary>
    /// Gets Windows machine ID from registry.
    /// </summary>
    [SupportedOSPlatform("windows")]
    private static string GetWindowsMachineId()
    {
        try
        {
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
            {
                if (key == null)
                {
                    System.Diagnostics.Debug.WriteLine("Warning: Could not open registry key for MachineGuid.");
                    return System.Net.Dns.GetHostName();
                }
                var machineGuid = key.GetValue("MachineGuid") as string;
                return machineGuid ?? System.Net.Dns.GetHostName();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Warning: Exception reading MachineGuid: {ex.Message}");
            return System.Net.Dns.GetHostName();
        }
    }

    /// <summary>
    /// Gets macOS machine ID from system.
    /// </summary>
    [SupportedOSPlatform("macos")]
    private static string GetMacMachineId()
    {
        try
        {
            // Note: UseShellExecute = false doesn't support shell pipes, so don't use | grep
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "ioreg",
                Arguments = "-rd1 -c IOPlatformExpertDevice",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = System.Diagnostics.Process.Start(psi))
            {
                if (process != null)
                {
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(5000); // Timeout after 5 seconds

                    if (!string.IsNullOrEmpty(output))
                    {
                        // Find the IOPlatformUUID line and parse it
                        foreach (var line in output.Split('\n'))
                        {
                            if (line.Contains("IOPlatformUUID", StringComparison.OrdinalIgnoreCase))
                            {
                                var parts = line.Split('=');
                                if (parts.Length > 1)
                                {
                                    var uuid = parts[1].Trim().Trim('"');
                                    if (!string.IsNullOrEmpty(uuid) && uuid.Length >= 16)
                                        return uuid;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // Ignore and fall through to hostname
        }

        return System.Net.Dns.GetHostName();
    }

    /// <summary>
    /// Gets Linux machine ID from /etc/machine-id or /var/lib/dbus/machine-id.
    /// </summary>
    [SupportedOSPlatform("linux")]
    private static string GetLinuxMachineId()
    {
        try
        {
            var machineIdPath = "/etc/machine-id";
            if (!File.Exists(machineIdPath))
                machineIdPath = "/var/lib/dbus/machine-id";

            if (File.Exists(machineIdPath))
            {
                var id = File.ReadAllText(machineIdPath).Trim();
                if (!string.IsNullOrEmpty(id))
                    return id;
            }
        }
        catch
        {
            // Ignore and fall through to hostname
        }

        return System.Net.Dns.GetHostName();
    }
}

/// <summary>
/// Exception thrown when encryption or decryption operations fail.
/// </summary>
public class CryptoException : Exception
{
    public CryptoException(string message) : base(message) { }
    public CryptoException(string message, Exception innerException) : base(message, innerException) { }
}
