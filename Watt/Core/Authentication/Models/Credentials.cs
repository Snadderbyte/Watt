using System;

namespace Watt.Core.Authentication;

/// <summary>
/// Base class for authentication credentials.
/// Extend this when adding new authentication methods.
/// </summary>
public abstract class Credentials
{
    public abstract AuthenticationMethod Method { get; }

    public required string EnvironmentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

