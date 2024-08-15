using System.Diagnostics;
using Duende.Bff.Blazor.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;

// This is based on the PersistingServerAuthenticationStateProvider from ASP.NET
// 8's templates.

// Future TODO - In .NET 9, the types added by the template are getting moved
// into ASP.NET itself, so we could potentially extend those instead of copying
// the template.

namespace Duende.Bff.Blazor;

// This is a server-side AuthenticationStateProvider that uses
// PersistentComponentState to flow the authentication state to the client which
// is then used to initialize the authentication state in the WASM application. 
public sealed class BffServerAuthenticationStateProvider : ServerAuthenticationStateProvider, IDisposable
{
    private readonly IClaimsService claimsService;
    private readonly IAuthenticationPropertiesProvider authenticationProperties;
    private readonly PersistentComponentState state;
    private readonly NavigationManager navigation;
    private readonly ILogger<BffServerAuthenticationStateProvider> logger;

    private readonly PersistingComponentStateSubscription subscription;

    private Task<AuthenticationState>? authenticationStateTask;

    public BffServerAuthenticationStateProvider(
        IClaimsService claimsService,
        IAuthenticationPropertiesProvider authenticationProperties,
        PersistentComponentState persistentComponentState,
        NavigationManager navigation,
        ILogger<BffServerAuthenticationStateProvider> logger)
    {
        this.claimsService = claimsService;
        this.authenticationProperties = authenticationProperties;
        this.state = persistentComponentState;
        this.navigation = navigation;
        this.logger = logger;

        AuthenticationStateChanged += OnAuthenticationStateChanged;
        subscription = state.RegisterOnPersisting(OnPersistingAsync, RenderMode.InteractiveWebAssembly);
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        authenticationStateTask = task;
    }

    private async Task OnPersistingAsync()
    {
        if (authenticationStateTask is null)
        {
            throw new UnreachableException($"Authentication state not set in {nameof(OnPersistingAsync)}().");
        }

        var authenticationState = await authenticationStateTask;

        var userClaims = await claimsService.GetUserClaimsAsync(authenticationState.User, authenticationProperties.Properties);
        var managementClaims = await claimsService.GetManagementClaimsAsync(new Uri(navigation.BaseUri).AbsolutePath, authenticationState.User, authenticationProperties.Properties);

        var claims = userClaims.Concat(managementClaims).Select(c => new ClaimLite
        {
            Type = c.type,
            Value = c.value?.ToString(),
            // TODO - Revisit ValueType. Consider consolidation of ClaimLite and ClaimRecord
            ValueType = null // c.ValueType = c.ValueType == ClaimValueTypes.String ? null : c.ValueType
        }).ToArray();

        var principal = new ClaimsPrincipalLite
        {
            AuthenticationType = authenticationState.User.Identity!.AuthenticationType,
            NameClaimType = authenticationState.User.Identities.First().NameClaimType,
            RoleClaimType = authenticationState.User.Identities.First().RoleClaimType,
            Claims = claims
        };

        logger.LogDebug("Persisting Authentication State");
        
        state.PersistAsJson(nameof(ClaimsPrincipalLite), principal);
    
    }

    public void Dispose()
    {
        subscription.Dispose();
        AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}