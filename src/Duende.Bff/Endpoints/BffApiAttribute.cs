// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;

namespace Duende.Bff
{
    /// <summary>
    /// Decorates a controller as a local BFF API endpoint
    /// This allows the BFF midleware to automatically add the antiforgery header checks as well as 302 to 401 conversion
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class BffApiAttribute : Attribute
    {
        /// <summary>
        /// specifies if anti-forgery check should be disbled (not recommended)
        /// </summary>
        public bool DisableAntiForgeryCheck { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="disableAntiForgeryCheck"></param>
        public BffApiAttribute(bool disableAntiForgeryCheck = false)
        {
            DisableAntiForgeryCheck = disableAntiForgeryCheck;
        }
    }
}