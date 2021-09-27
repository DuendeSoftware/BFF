// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Duende.Bff.Yarp
{
    public static class BffYarpBuilderExtensions
    {
        public static BffBuilder AddReverseProxy(this BffBuilder builder)
        {
            // reverse proxy related
            builder.Services.TryAddSingleton<IHttpMessageInvokerFactory, DefaultHttpMessageInvokerFactory>();
            builder.Services.TryAddSingleton<IHttpTransformerFactory, DefaultHttpTransformerFactory>();

            return builder;
        }
    }
}