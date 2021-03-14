using System;

namespace Duende.Bff
{
    /// <summary>
    /// Decorates a controller as a local BFF API endpoint
    /// This allows the BFF midleware to add the antiforgery header checks as well as 302 to 401 conversion
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BffLocalApiEndpointAttribute : Attribute
    {
        /// <summary>
        /// specifies if anti-forgery check should be disbled (not recommended)
        /// </summary>
        public bool DisableAntiForgeryCheck { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="disableAntiForgeryCheck"></param>
        public BffLocalApiEndpointAttribute(bool disableAntiForgeryCheck = false)
        {
            DisableAntiForgeryCheck = disableAntiForgeryCheck;
        }
    }
}