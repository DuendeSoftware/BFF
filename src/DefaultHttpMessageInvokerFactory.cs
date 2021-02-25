using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;

namespace Duende.Bff
{
    public class DefaultHttpMessageInvokerFactory : IDefaultHttpMessageInvokerFactory
    {
        private readonly ConcurrentDictionary<string, HttpMessageInvoker> _clients = new();
        
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