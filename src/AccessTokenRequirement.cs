namespace Duende.Bff
{
    public enum AccessTokenRequirement
    {
        None,
        OptionalUserToken,
        RequireUserToken,
        RequireClientToken
    }
}