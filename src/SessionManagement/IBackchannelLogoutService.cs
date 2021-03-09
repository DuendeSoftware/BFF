using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Duende.Bff
{
    public interface IBackchannelLogoutService
    {
        Task ProcessRequequestAsync(HttpContext context);
    }
}