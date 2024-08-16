// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

namespace Duende.Bff.Blazor.Client;

/// <summary>
///     Options for Blazor BFF
/// </summary>
public class BffBlazorOptions
{
    /// <summary>
    ///     The base path to use for remote APIs.
    /// </summary>
    public string RemoteApiPath { get; set; } = "remote-apis/";

    /// <summary>
    ///     The base address to use for remote APIs. If unset (the default), the
    ///     blazor hosting environment's base address is used.
    /// </summary>
    public string? RemoteApiBaseAddress { get; set; } = null;

    public int StateProviderPollingDelay { get; set; } = 1000;
    public int StateProviderPollingInterval { get; set; } = 5000;
}