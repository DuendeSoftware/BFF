// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Duende.Bff.EntityFramework
{
    internal readonly struct Identifier : IEquatable<Identifier>
    {
        public Identifier(string applicationDiscriminator, string key, IdentifierKind kind)
        {
            ApplicationDiscriminator = applicationDiscriminator;
            Key = key;
            Kind = kind;
        }

        public string ApplicationDiscriminator { get; }

        public string Key { get; }

        public IdentifierKind Kind { get; }

        public override string ToString() =>
            $"{ApplicationDiscriminator}:{Kind}:{Key}";

        public static implicit operator string(Identifier id) => id.ToString();

        public bool Equals(Identifier other)
        {
            return ApplicationDiscriminator == other.ApplicationDiscriminator &&
                Key == other.Key &&
                Kind == other.Kind;
        }

        public override bool Equals(object? obj)
        {
            return obj is Identifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ApplicationDiscriminator, Key, (int) Kind);
        }

        public static bool TryParse(string key, [NotNullWhen(true)] out Identifier? identifier)
        {
            var values = key.Split(":");
            if (values.Length == 3 && Enum.TryParse(values[1], out IdentifierKind kind))
            {
                identifier = new Identifier(values[0], values[2], kind);
                return true;
            }

            identifier = null;
            return false;
        }
    }
}