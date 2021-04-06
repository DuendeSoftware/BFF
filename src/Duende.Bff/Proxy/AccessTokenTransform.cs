using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Service.RuntimeModel.Transforms;

namespace Duende.Bff
{
    public class AccessTokenTransform : RequestTransform
    {
        private readonly string _accessToken;

        public AccessTokenTransform(string accessToken)
        {
            _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        }

        public override ValueTask ApplyAsync(RequestTransformContext context)
        {
            context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return ValueTask.CompletedTask;
        }
    }
}