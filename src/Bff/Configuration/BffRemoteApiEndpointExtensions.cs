// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using System.Linq;
using Duende.Bff;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for BFF remote API endpoints
/// </summary>
public static class BffRemoteApiEndpointExtensions
{
    private static BffRemoteApiEndpointMetadata GetBffRemoteApiEndpointMetadata(this EndpointBuilder builder)
    {
        if(builder.Metadata.First(m => m.GetType() == typeof(BffRemoteApiEndpointMetadata)) 
            is not BffRemoteApiEndpointMetadata metadata)
        {
            throw new InvalidOperationException("no metadata found");
        }
        return metadata;
    }

    /// <summary>
    /// Specifies the access tokens requirements for an endpoint
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="type"></param>
    /// <typeparam name="TBuilder"></typeparam>
    /// <returns></returns>
    public static TBuilder RequireAccessToken<TBuilder>(this TBuilder builder, TokenType type = TokenType.User) where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(endpointBuilder =>
        {
            var metadata = endpointBuilder.GetBffRemoteApiEndpointMetadata();
            metadata.RequiredTokenType = type;
        });
        return builder;
    }

    /// <summary>
    /// Specifies a type to use for access token retrieval.
    /// </summary>
    /// <typeparam name="TRetriever"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">When invoked multiple times for the same endpoint.</exception>
    public static IEndpointConventionBuilder WithAccessTokenRetriever<TRetriever>(this IEndpointConventionBuilder builder) 
        where TRetriever : IAccessTokenRetriever
    {
        builder.Add(endpointBuilder =>
        {
            var metadata = endpointBuilder.GetBffRemoteApiEndpointMetadata();
            metadata.AccessTokenRetriever = typeof(TRetriever);
        });
        return builder;
    }

    /// <summary>
    /// Allows for anonymous access with an optional user token for an endpoint
    /// </summary>
    /// <param name="builder"></param>
    /// <typeparam name="TBuilder"></typeparam>
    /// <returns></returns>
    public static TBuilder WithOptionalUserAccessToken<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(endpointBuilder =>
        {
            var metadata = endpointBuilder.GetBffRemoteApiEndpointMetadata();
            metadata.OptionalUserToken = true;
        });

        return builder;
    }

    /// <summary>
    /// Allows for setting a UserAccessTokenParameter
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="bffUserAccessTokenParameters"></param>
    /// <typeparam name="TBuilder"></typeparam>
    /// <returns></returns>
    public static TBuilder WithUserAccessTokenParameter<TBuilder>(this TBuilder builder, BffUserAccessTokenParameters bffUserAccessTokenParameters) where TBuilder : IEndpointConventionBuilder
    {
        builder.Add(endpointBuilder =>
        {
            var metadata = endpointBuilder.GetBffRemoteApiEndpointMetadata();
            metadata.BffUserAccessTokenParameters = bffUserAccessTokenParameters;
        });

        return builder;
    }
}