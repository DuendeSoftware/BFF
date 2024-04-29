using Blazor.Client;

public class ClientRenderModeContext : IRenderModeContext
{
    public RenderMode GetMode() => RenderMode.Client;
}
