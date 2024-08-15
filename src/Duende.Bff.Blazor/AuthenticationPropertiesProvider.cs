using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

public class AuthenticationPropertiesProvider : IAuthenticationPropertiesProvider 
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationPropertiesProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task Initialize()
    {
        if (_httpContextAccessor.HttpContext != null)
        {
            var authResult = await _httpContextAccessor.HttpContext.AuthenticateAsync();
            _properties = authResult?.Properties;
        }
        else
        {
            throw new InvalidOperationException("Attempt to capture authentication properties when no HTTP context is available");
        }
    }

    private AuthenticationProperties? _properties;

    public AuthenticationProperties Properties 
    { 
        get 
        {
            if (_properties == null)
            {
                throw new InvalidOperationException("Attempt to retrieve AuthenticationProperties from an uninitialized AuthenticationPropertiesProvider.");
            }
            return _properties;
        }
        private set => _properties = value; 
    }
}

public interface IAuthenticationPropertiesProvider
{
    AuthenticationProperties Properties { get; }
    Task Initialize();
}
