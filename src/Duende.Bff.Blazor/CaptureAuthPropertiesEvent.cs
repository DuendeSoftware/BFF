using Microsoft.AspNetCore.Authentication.Cookies;

namespace Duende.Bff.Blazor;

public class CaptureAuthPropertiesEvent : CookieAuthenticationEvents
{
    private readonly IAuthenticationPropertiesProvider _propsProvider;

    public CaptureAuthPropertiesEvent(IAuthenticationPropertiesProvider propsProvider)
    {
        _propsProvider = propsProvider;
    }
    public override async Task SigningIn(CookieSigningInContext context)
    {
        await _propsProvider.Initialize();
    }
}