namespace Duende.Bff
{
    public enum AccessTokenRequirement
    {
        // no requirement
        None,
        
        // forward user token if available
        OptionalUserToken,
        
        // require a user token
        RequireUserToken,
        
        // require a client token
        RequireClientToken
    }
}