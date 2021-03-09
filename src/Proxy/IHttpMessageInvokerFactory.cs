using System.Net.Http;

namespace Duende.Bff
{
    public interface IHttpMessageInvokerFactory
    {
        HttpMessageInvoker CreateClient(string localPath);
    }
}