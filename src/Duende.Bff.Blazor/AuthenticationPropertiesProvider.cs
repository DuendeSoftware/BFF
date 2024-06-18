using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;

public class AuthenticationPropertiesProvider : IAuthenticationPropertiesProvider 
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationPropertiesProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
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

public class AuthenticationPropertiesCircuitHandler : CircuitHandler
{
    private readonly ICircuitAuthenticationProperties _circuitAuthenticationProperties;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationPropertiesCircuitHandler(ICircuitAuthenticationProperties circuitAuthenticationProperties, IHttpContextAccessor httpContextAccessor)
    {
        _circuitAuthenticationProperties = circuitAuthenticationProperties;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var authResult = await _httpContextAccessor.HttpContext?.AuthenticateAsync();
        _circuitAuthenticationProperties.Properties = authResult?.Properties;
    }
}
