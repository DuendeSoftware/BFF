using System.Net.Http;

namespace Duende.Bff
{
    public interface IDefaultHttpMessageInvokerFactory
    {
        HttpMessageInvoker CreateClient(string localPath);
    }
}