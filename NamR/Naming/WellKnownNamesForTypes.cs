// <copyright file="WellKnownNamesForTypes.cs" company="Ralf 'bexxx' Beckers">
// Copyright (c) Ralf 'bexxx' Beckers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace NamR
{
    using System.Collections.Generic;

    internal static class WellKnownNamesForTypes
    {
        public static Dictionary<string, string> TypeToNameMapping { get; } =
            new Dictionary<string, string>
            {
                { "Guid", "id" }
            };

        public static Dictionary<string, string> TypeToSuffixMapping { get; } =
            new Dictionary<string, string>
            {
                { "Guid", "Id" }
            };
    }
}
