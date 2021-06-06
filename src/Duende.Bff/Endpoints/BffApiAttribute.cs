// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;

namespace Duende.Bff
{
    /// <summary>
    /// Decorates a controller as a local BFF API endpoint
    /// This allows the BFF midleware to automatically add the anti-forgery header checks as well as 302 to 401 conversion
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BffApiAttribute : Attribute
    {
        /// <summary>
        /// specifies if anti-forgery check is required
        /// </summary>
        public bool RequireAntiForgeryCheck { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="requireAntiForgeryCheck">Specifies if the antiforgery header gets checked</param>
        public BffApiAttribute(bool requireAntiForgeryCheck = true)
        {
            RequireAntiForgeryCheck = requireAntiForgeryCheck;
        }
    }
}