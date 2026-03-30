# Cross-Platform Encryption Migration

## Summary
Successfully migrated the Watt authentication system from Windows-only DPAPI (Data Protection API) to cross-platform AES encryption that works on Windows, macOS, and Linux.

## Changes Made

### 1. New Encryption Module
Created a new `Encryption` namespace with two components:

#### `ICryptoProvider.cs`
- Interface defining encryption/decryption contract
- Allows for future encryption provider implementations
- Methods: `Encrypt(plaintext)` and `Decrypt(ciphertext)`

#### `AesEncryptionProvider.cs`
- Implements `ICryptoProvider` using AES-256 encryption
- Cross-platform compatible (Windows, macOS, Linux)
- Key derivation using PBKDF2 from:
  - **Machine identifier**: Platform-specific (Registry on Windows, `ioreg` on macOS, `/etc/machine-id` on Linux)
  - **User name**: Retrieved via `Environment.UserName`
- Per-user, per-machine encryption (credentials not transferable)
- Graceful fallbacks to hostname if machine ID retrieval fails

### 2. Updated CredentialManager
- Replaced `System.Security.Cryptography.ProtectedData` (DPAPI) imports
- Added dependency on `ICryptoProvider`
- Instantiates `AesEncryptionProvider` in constructor
- Updated `EncryptData()` and `DecryptData()` methods to use the new provider
- No API changes - drop-in replacement

### 3. Updated Documentation
- README.md updated to reflect cross-platform AES encryption
- Security Considerations section now documents:
  - AES-256 encryption with PBKDF2 key derivation
  - Per-user, per-machine encryption
  - Cross-platform support
- Troubleshooting section expanded with:
  - Platform-specific guidance (Linux, macOS, Windows)
  - Machine identifier retrieval information
  - AppData paths for all platforms

## Technical Details

### Encryption Method
- **Algorithm**: AES-256
- **Mode**: CBC (implicit in .NET's Aes.Create())
- **IV**: Derived from machine/user identifiers
- **Key Derivation**: PBKDF2-SHA256 with 10,000 iterations
- **Encoding**: Base64 for storage

### Machine Identification
- **Windows**: `SOFTWARE\Microsoft\Cryptography\MachineGuid` registry value
- **macOS**: UUID from `ioreg -rd1 -c IOPlatformExpertDevice` command
- **Linux**: `/etc/machine-id` or `/var/lib/dbus/machine-id` file
- **Fallback**: System hostname

### Benefits
1. ✅ Works on Windows, macOS, and Linux
2. ✅ Per-user encryption (credentials tied to current user)
3. ✅ Per-machine encryption (credentials not transferable between machines)
4. ✅ Uses industry-standard PBKDF2 for key derivation
5. ✅ No external dependencies required
6. ✅ Backward compatibility achievable by migrating old DPAPI-encrypted files

## Build Status
- ✅ Project builds successfully with zero warnings and errors
- ✅ All new encryption classes compile without issues
- ✅ CredentialManager integrates seamlessly

## Migration Notes

### For Existing Users
**Important**: Old DPAPI-encrypted credentials will not be automatically readable with the new AES encryption. Consider:

1. **One-time migration approach** (if implementing):
   - Detect old DPAPI-encrypted files
   - Decrypt with DPAPI (on Windows only)
   - Re-encrypt with new AES provider
   - Store migrated credentials

2. **Clean slate approach** (recommended):
   - Clear old credential files
   - Re-authenticate when application starts
   - Credentials stored with new AES encryption

### Platform-Specific Setup
No additional setup required. The system automatically:
- Detects the operating system
- Retrieves platform-specific machine identifier
- Derives encryption key
- Encrypts/decrypts credentials transparently

## File Locations

### Windows
- `%APPDATA%\Watt\watt_credentials.json` (encrypted)
- `%APPDATA%\Watt\watt_environments.json` (plain JSON)

### macOS
- `~/Library/Application Support/Watt/watt_credentials.json` (encrypted)
- `~/Library/Application Support/Watt/watt_environments.json` (plain JSON)

### Linux
- `~/.config/Watt/watt_credentials.json` (encrypted)
- `~/.config/Watt/watt_environments.json` (plain JSON)
