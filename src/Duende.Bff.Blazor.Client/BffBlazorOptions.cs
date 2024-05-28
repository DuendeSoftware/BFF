namespace Duende.Bff.Blazor.Wasm;

public class BffBlazorOptions
{
    // TODO - Consider using PathString here (would require more dependencies?)
    public string RemoteApiPath { get; set; } = "remote-apis/";
    public string? RemoteApiBaseAddress { get; set; } = null;
}