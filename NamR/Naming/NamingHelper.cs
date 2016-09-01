﻿// <copyright file="NamingHelper.cs" company="Ralf 'bexxx' Beckers">
// Copyright (c) Ralf 'bexxx' Beckers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace NamR
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.InteropServices;

    [Guid("3CA5A739-A839-438D-A4DB-F09EE613003E")]
    public static class NamingHelper
    {
        public static IEnumerable<string> CreateNameProposals(string typeName, bool isUppercase)
        {
            return CreateNameProposals(typeName, isUppercase, string.Empty);
        }

        public static IEnumerable<string> CreateNameProposals(string typeName, bool isUppercase, string beginning)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                throw new ArgumentException("Argument cannot be null, empty or whitespace.", nameof(typeName));
            }

            if (typeName.Length <= 2)
            {
                return Enumerable.Empty<string>();
            }

            // remove I if it's an interface
            if (IsInterface(typeName))
            {
                typeName = typeName.Substring(1);
            }

            var results = new List<string>();

            if (char.IsUpper(typeName[0]))
            {
                // e.g. Guid -> id
                var commonName = ProposeCommonNames(typeName);
                if (!string.IsNullOrEmpty(commonName))
                {
                    results.Add(commonName);
                }

                var withCommonSuffix = AppendCommonSuffix(typeName, beginning);
                if (!string.IsNullOrEmpty(withCommonSuffix))
                {
                    results.Add(withCommonSuffix);
                }

                results.Add(char.ToLower(typeName[0], CultureInfo.CurrentCulture) + typeName.Substring(1));
            }

            if (HasMultipleNameParts(typeName))
            {
                results.Add(GetAbreviatedName(typeName));
            }

            if (isUppercase)
            {
                return results.Select(s => char.ToUpper(s[0], CultureInfo.CurrentCulture) + s.Substring(1));
            }
            else
            {
                return results;
            }
        }

        internal static string ProposeCommonNames(string typeName)
        {
            string commonName = null;
            if (WellKnownNamesForTypes.TypeToNameMapping.TryGetValue(typeName, out commonName))
            {
                return commonName;
            }

            return commonName;
        }

        internal static string AppendCommonSuffix(string typeName, string beginning)
        {
            string commonSuffix = null;
            if (string.IsNullOrEmpty(beginning))
            {
                return null;
            }

            if (WellKnownNamesForTypes.TypeToSuffixMapping.TryGetValue(typeName, out commonSuffix))
            {
                return beginning + commonSuffix;
            }

            return commonSuffix;
        }

        internal static string GetAbreviatedName(string typeName)
        {
            return new string(typeName.Where(c => char.IsUpper(c)).ToArray()).ToLower(CultureInfo.CurrentCulture);
        }

        internal static bool IsInterface(string typeName)
        {
            if (char.IsLower(typeName[0]))
            {
                return false;
            }

            return typeName.Length > 2 && typeName[0] == 'I' && char.IsUpper(typeName[1]);
        }

        internal static bool HasMultipleNameParts(string typeName)
        {
            if (char.IsLower(typeName[0]))
            {
                return false;
            }

            bool wasLowerCase = false;
            for (var i = 1; i < typeName.Length; i++)
            {
                if (!wasLowerCase && char.IsLower(typeName[i]))
                {
                    wasLowerCase = true;
                }

                if (wasLowerCase && char.IsUpper(typeName[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
