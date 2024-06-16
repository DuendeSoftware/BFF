namespace Duende.Bff.Blazor.Wasm;

/// <summary>
/// Serialization friendly ClaimsPrincipal
/// </summary>
public class ClaimsPrincipalLite
{
    /// <summary>
    /// The authentication type
    /// </summary>
    public string? AuthenticationType { get; init; }

    /// <summary>
    /// The name claim type
    /// </summary>
    public string? NameClaimType { get; init; }

    /// <summary>
    /// The role claim type
    /// </summary>
    public string? RoleClaimType { get; init; }

    /// <summary>
    /// The claims
    /// </summary>
    public ClaimLite[] Claims { get; init; } = default!;
}
