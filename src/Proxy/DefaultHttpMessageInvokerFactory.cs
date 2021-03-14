using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;

namespace Duende.Bff
{
    /// <summary>
    /// Default implementation of the message invoker factory.
    /// This implementation creates one message invoke per remote API endpoint
    /// </summary>
    public class DefaultHttpMessageInvokerFactory : IHttpMessageInvokerFactory
    {
        private readonly ConcurrentDictionary<string, HttpMessageInvoker> _clients =
            new ConcurrentDictionary<string, HttpMessageInvoker>();

        /// <inheritdoc />
        public HttpMessageInvoker CreateClient(string localPath)
        {
            return _clients.GetOrAdd(localPath, (key) =>
            {
                return new HttpMessageInvoker(new SocketsHttpHandler()
                {
                    UseProxy = false,
                    AllowAutoRedirect = false,
                    AutomaticDecompression = DecompressionMethods.None,
                    UseCookies = false
                });
            });
        }
    }
}