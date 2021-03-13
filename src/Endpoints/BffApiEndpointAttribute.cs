using System;

namespace Duende.Bff
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BffLocalApiEndpointAttribute : Attribute
    {
        public bool DisableAntiforgeryCheck { get; }

        public BffLocalApiEndpointAttribute(bool disableAntiforgeryCheck = false)
        {
            DisableAntiforgeryCheck = disableAntiforgeryCheck;
        }
    }
}