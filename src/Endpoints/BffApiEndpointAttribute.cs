using System;

namespace Duende.Bff
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BffApiEndpointAttribute : Attribute
    {
        public bool DisableAntiforgeryCheck { get; }

        public BffApiEndpointAttribute(bool disableAntiforgeryCheck = false)
        {
            DisableAntiforgeryCheck = disableAntiforgeryCheck;
        }
    }
}