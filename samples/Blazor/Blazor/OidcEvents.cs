using Duende.AccessTokenManagement.OpenIdConnect;
using Duende.Bff;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace Blazor;

// TODO - Move to Duende.BFF.Blazor
public class OidcEvents(IUserTokenStore store, ILogger<BffOpenIdConnectEvents> logger) : BffOpenIdConnectEvents(logger)
{
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var exp = DateTimeOffset.UtcNow.AddSeconds(Double.Parse(context.TokenEndpointResponse!.ExpiresIn));

        await store.StoreTokenAsync(context.Principal!, new UserToken
        {
            AccessToken = context.TokenEndpointResponse.AccessToken,
            AccessTokenType = context.TokenEndpointResponse.TokenType,
            Expiration = exp,
            RefreshToken = context.TokenEndpointResponse.RefreshToken,
            Scope = context.TokenEndpointResponse.Scope
        });

        await base.TokenValidated(context);
    }

    public override Task UserInformationReceived(UserInformationReceivedContext context)
    {
        return base.UserInformationReceived(context);
    }
}


public class MyAuthState : ServerAuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return base.GetAuthenticationStateAsync();
    }

    public MyAuthState()
    {
        AuthenticationStateChanged += MyAuthState_AuthenticationStateChanged;
    }

    private void MyAuthState_AuthenticationStateChanged(Task<AuthenticationState> task)
    {
    }
}