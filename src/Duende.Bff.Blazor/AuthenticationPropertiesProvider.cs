using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

public class AuthenticationPropertiesProvider : IAuthenticationPropertiesProvider 
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationPropertiesProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        
        // Invoke the properties getter to ensure that we have the properties
        // captured as soon as this provider is added to the DI scope This
        // relies on the AuthenticationPropertiesProvider being added as a
        // Scoped dependency.
        _ = Properties;
    }

    private AuthenticationProperties? _properties;

    public AuthenticationProperties? Properties 
    { 
        get 
        {
            if (_properties == null)
            {
                if (_httpContextAccessor.HttpContext != null)
                {
                    var authResult = _httpContextAccessor.HttpContext?.AuthenticateAsync().Result;
                    _properties = authResult?.Properties;
                }
                // TODO - handle this error
            }
            return _properties;
        }
        set => _properties = value; 
    }
}

public interface IAuthenticationPropertiesProvider
{
    AuthenticationProperties? Properties { get; set; }
}
