using System;

namespace Watt.Core.Authentication.Encryption;

/// <summary>
/// Interface for cross-platform encryption/decryption of sensitive data.
/// </summary>
public interface ICryptoProvider
{
    /// <summary>
    /// Encrypts plaintext data to a base64-encoded string.
    /// </summary>
    /// <param name="plaintext">The plaintext to encrypt</param>
    /// <returns>Base64-encoded ciphertext</returns>
    string Encrypt(string plaintext);

    /// <summary>
    /// Decrypts base64-encoded ciphertext back to plaintext.
    /// </summary>
    /// <param name="ciphertext">The base64-encoded ciphertext</param>
    /// <returns>Decrypted plaintext</returns>
    string Decrypt(string ciphertext);
}
