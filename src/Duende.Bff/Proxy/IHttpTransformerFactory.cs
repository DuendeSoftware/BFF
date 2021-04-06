using Yarp.ReverseProxy.Service.Proxy;

namespace Duende.Bff
{
    /// <summary>
    /// Factory for creating a HTTP transformer for outgoing remote BFF API calls
    /// </summary>
    public interface IHttpTransformerFactory
    {
        /// <summary>
        /// Creates a HTTP transformer based on the local path
        /// </summary>
        /// <param name="localPath">Local path the remote API is mapped to</param>
        /// <param name="accessToken">The access token to attach to the request (if present)</param>
        /// <returns></returns>
        HttpTransformer CreateClient(string localPath, string accessToken = null);
    }
}