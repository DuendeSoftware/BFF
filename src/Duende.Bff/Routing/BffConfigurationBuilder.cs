// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Duende.Bff;

/// <summary>
/// Builder for adding BFF endpoints to the routing table.
/// </summary>
public class BffConfigurationBuilder
{
    private readonly BffEndpointDataSource _bffEndpointDataSource;

    internal BffConfigurationBuilder(PathString basePath, BffEndpointDataSource bffEndpointDataSource)
    {
        _bffEndpointDataSource = bffEndpointDataSource;
        _bffEndpointDataSource.BasePath = basePath;
    }

    /// <summary>
    /// Adds login.
    /// </summary>
    public void AddLogin()
    {
        AddLogin(Constants.ManagementEndpoints.Login);
    }
    /// <summary>
    /// Adds login.
    /// </summary>
    public void AddLogin(PathString path)
    {
        _bffEndpointDataSource.Map<ILoginService>(path);
    }

    /// <summary>
    /// Adds silent login.
    /// </summary>
    public void AddSilentLogin()
    {
        AddSilentLogin(Constants.ManagementEndpoints.SilentLogin, Constants.ManagementEndpoints.SilentLoginCallback);
    }
    /// <summary>
    /// Adds silent login.
    /// </summary>
    public void AddSilentLogin(PathString login, PathString callback)
    {
        _bffEndpointDataSource.Map<ISilentLoginService>(login);
        _bffEndpointDataSource.Map<ISilentLoginCallbackService>(callback);
    }



    /// <summary>
    /// Adds logout.
    /// </summary>
    public void AddLogout()
    {
        AddLogout(Constants.ManagementEndpoints.Logout);
    }
    /// <summary>
    /// Adds logout.
    /// </summary>
    public void AddLogout(PathString path)
    {
        _bffEndpointDataSource.Map<ILogoutService>(path);
    }


    /// <summary>
    /// Adds user.
    /// </summary>
    public void AddUser()
    {
        AddUser(Constants.ManagementEndpoints.User);
    }
    /// <summary>
    /// Adds user.
    /// </summary>
    public void AddUser(PathString path)
    {
        _bffEndpointDataSource.Map<IUserService>(path);
    }


    /// <summary>
    /// Adds back-channel logout.
    /// </summary>
    public void AddBackchannelLogout()
    {
        AddBackchannelLogout(Constants.ManagementEndpoints.BackChannelLogout);
    }
    /// <summary>
    /// Adds back-channel logout.
    /// </summary>
    public void AddBackchannelLogout(PathString path)
    {
        _bffEndpointDataSource.Map<IBackchannelLogoutService>(path);
    }


    /// <summary>
    /// Adds diagnostics.
    /// </summary>
    public void AddDiagnostics()
    {
        AddDiagnostics(Constants.ManagementEndpoints.Diagnostics);
    }
    /// <summary>
    /// Adds diagnostics.
    /// </summary>
    public void AddDiagnostics(PathString path)
    {
        _bffEndpointDataSource.Map<IDiagnosticsService>(path);
    }
}
