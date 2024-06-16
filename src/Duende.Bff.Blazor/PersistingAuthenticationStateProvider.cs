using System.Diagnostics;
using Duende.Bff.Blazor.Wasm;
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

// This is a server-side AuthenticationStateProvider that uses PersistentComponentState to flow the
// authentication state to the client which is then used to initialize the authentication state in the 
// WASM application. 
public sealed class PersistingServerAuthenticationStateProvider : ServerAuthenticationStateProvider, IDisposable
{
    private readonly PersistentComponentState state;
    private readonly ILogger<PersistingServerAuthenticationStateProvider> logger;

    private readonly PersistingComponentStateSubscription subscription;

    private Task<AuthenticationState>? authenticationStateTask;

    public PersistingServerAuthenticationStateProvider(PersistentComponentState persistentComponentState, ILogger<PersistingServerAuthenticationStateProvider> logger)
    {
        state = persistentComponentState;
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
        var principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated == true)
        {
            logger.LogInformation("Persisting Authentication State");
            
            state.PersistAsJson(nameof(ClaimsPrincipalLite), principal.ToClaimsPrincipalLite());
        } else
        {
            logger.LogInformation("NOT Persisting Authentication State");
        }
    }

    public void Dispose()
    {
        subscription.Dispose();
        AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}