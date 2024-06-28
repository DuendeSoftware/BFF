using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;

namespace Duende.Bff.Blazor.Client;

public class BffClientAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly TimeSpan UserCacheRefreshInterval = TimeSpan.FromSeconds(60);

    private readonly HttpClient _client;
    private readonly ILogger<BffClientAuthenticationStateProvider> _logger;
    private readonly BffBlazorOptions _options;

    private DateTimeOffset _userLastCheck = DateTimeOffset.MinValue;
    private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());

    public BffClientAuthenticationStateProvider(
        PersistentComponentState state,
        IHttpClientFactory factory,
        IOptions<BffBlazorOptions> options,
        ILogger<BffClientAuthenticationStateProvider> logger)
    {
        _client = factory.CreateClient("BffAuthenticationStateProvider");
        _logger = logger;
        _cachedUser = GetPersistedUser(state);
        if (_cachedUser.Identity?.IsAuthenticated == true) 
        {
            _userLastCheck = DateTimeOffset.Now;
        }
        _options = options.Value;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = await GetUser();
        var state = new AuthenticationState(user);

        // checks periodically for a session state change and fires event
        // this causes a round trip to the server
        // adjust the period accordingly if that feature is needed
        if (user!.Identity!.IsAuthenticated)
        {
            _logger.LogInformation("starting background check..");
            Timer? timer = null;

            timer = new Timer(async _ =>
            {
                var currentUser = await GetUser(false);
                // Always notify that auth state has changed, because the user management claims change over time
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(currentUser)));

                if (currentUser!.Identity!.IsAuthenticated == false)
                {
                    _logger.LogInformation("user logged out");

                    if (timer != null)
                    {
                        await timer.DisposeAsync();
                    }
                }
            }, null, _options.StateProviderPollingDelay, _options.StateProviderPollingInterval);
        }

        return state;
    }

    private async ValueTask<ClaimsPrincipal> GetUser(bool useCache = true)
    {
        var now = DateTimeOffset.Now;
        if (useCache && now < _userLastCheck + UserCacheRefreshInterval)
        {
            _logger.LogDebug("Taking user from cache");
            return _cachedUser;
        }

        _logger.LogDebug("Fetching user");
        _cachedUser = await FetchUser();
        _userLastCheck = now;

        return _cachedUser;
    }

    record ClaimRecord(string Type, object Value);

    private async Task<ClaimsPrincipal> FetchUser()
    {
        try
        {
            _logger.LogInformation("Fetching user information.");
            var response = await _client.GetAsync("bff/user?slide=false");
            response.EnsureSuccessStatusCode();
            var claims = await response.Content.ReadFromJsonAsync<List<ClaimRecord>>();

            var identity = new ClaimsIdentity(
                nameof(BffClientAuthenticationStateProvider),
                "name",
                "role");

            if (claims != null)
            {
                foreach (var claim in claims)
                {
                    identity.AddClaim(new Claim(claim.Type, claim.Value.ToString() ?? "no value"));
                }    
            }

            return new ClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Fetching user failed.");
        }

        return new ClaimsPrincipal(new ClaimsIdentity());
    }

    private ClaimsPrincipal GetPersistedUser(PersistentComponentState state)
    {
        if (!state.TryTakeFromJson<ClaimsPrincipalLite>(nameof(ClaimsPrincipalLite), out var lite) || lite is null)
        {
            _logger.LogDebug("Failed to load persisted user.");
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        _logger.LogDebug("Persisted user loaded.");

        return lite.ToClaimsPrincipal();
    }
}
