namespace Duende.Bff.Blazor.Client;

// TODO - Consider consolidating this and Duende.Bff.ClaimLite

/// <summary>
/// Serialization friendly claim
/// </summary>
public class ClaimLite
{
    /// <summary>
    /// The type
    /// </summary>
    public string Type { get; init; } = default!;
        
    /// <summary>
    /// The value
    /// </summary>
    public string Value { get; init; } = default!;

    /// <summary>
    /// The value type
    /// </summary>
    public string? ValueType { get; init; }
}
