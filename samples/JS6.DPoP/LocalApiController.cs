// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace JS6.DPoP;
[Route("local")]
public class LocalApiController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public LocalApiController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [Route("self-contained")]
    [HttpGet]
    public IActionResult SelfContained()
    {
        var data = new
        {
            Message = "Hello from self-contained local API",
            User = User!.FindFirst("name")?.Value ?? User!.FindFirst("sub")!.Value
        };

        return Ok(data);
    }

    [Route("invokes-external-api")]
    [HttpGet]
    public async Task<IActionResult> InvokesExternalApisAsync()
    {
        var httpClient = _httpClientFactory.CreateClient("api");
        var stuff = await httpClient.GetAsync("/user-token");
        var moreStuff = await stuff.Content.ReadAsStringAsync();
        var thing = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(moreStuff);

        var data = new
        {
            Message = "Hello from local API that invokes a remote api",
            RemoteApiResponse = thing
        };

        return Ok(data);
    }
}
