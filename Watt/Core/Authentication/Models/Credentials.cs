using System;

namespace Watt.Core.Authentication;

/// <summary>
/// Base class for authentication credentials.
/// Extend this when adding new authentication methods.
/// </summary>
public abstract class Credentials
{
    /// <summary>
    /// The authentication method these credentials are for. Used to determine which provider can handle these credentials.
    /// </summary>
    public abstract AuthenticationMethod Method { get; }

    /// <summary>
    /// The environment ID these credentials are associated with.
    /// </summary>
    public required string EnvironmentId { get; set; }

    /// <summary>
    /// The date and time when these credentials were created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

