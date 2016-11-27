// <copyright file="NamingHelper.cs" company="Ralf 'bexxx' Beckers">
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
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [Guid("3CA5A739-A839-438D-A4DB-F09EE613003E")]
    public static class NamingHelper
    {
        public static IEnumerable<string> CreateNameProposals(string typeName, bool isUppercase, bool isMultiple)
        {
            return CreateNameProposals(typeName, isUppercase, isMultiple, string.Empty);
        }

        public static IEnumerable<string> CreateNameProposals(string typeName, bool isUppercase, bool isMultiple, string beginning)
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

                if (!WellKnownNamesForTypes.TypesToFilterOut.Contains(typeName))
                {
                    results.Add(char.ToLower(typeName[0], CultureInfo.CurrentCulture) + typeName.Substring(1));
                }
            }

            var withCommonSuffix = AppendCommonSuffix(typeName, beginning);
            results.AddRange(withCommonSuffix);

            if (HasMultipleNameParts(typeName))
            {
                results.Add(GetAbreviatedName(typeName));

                var parts = GetNameParts(typeName).ToList();
                var names = GetCombination(parts);
                results.AddRange(names);
            }

            IEnumerable<string> typedResults = results;

            if (isUppercase)
            {
                typedResults = results.Select(s => char.ToUpper(s[0], CultureInfo.CurrentCulture) + s.Substring(1));
            }

            if (isMultiple)
            {
                typedResults = typedResults.Select(s => s + "s");
            }

            return typedResults.Distinct().OrderByDescending(x => x.Length);
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

        internal static IEnumerable<string> AppendCommonSuffix(string typeName, string beginning)
        {
            if (string.IsNullOrEmpty(beginning))
            {
                return Enumerable.Empty<string>();
            }

            IEnumerable<string> commonSuffixes = null;
            if (WellKnownNamesForTypes.TypeToSuffixMapping.TryGetValue(typeName, out commonSuffixes))
            {
                return commonSuffixes.Select(s => beginning + s);
            }

            return Enumerable.Empty<string>();
        }

        internal static string GetAbreviatedName(string typeName)
        {
            return new string(typeName.Where(c => char.IsUpper(c)).ToArray()).ToLower(CultureInfo.CurrentCulture);
        }

        internal static IEnumerable<string> GetNameParts(string typeName)
        {
            if (typeName.Length < 2)
            {
                yield break;
            }

            // Get the indexes of every uppercase character that is followed by a lower case character
            var indexes = new List<int>();

            for (var i = 0; i < typeName.Length - 1; i++)
            {
                var current = typeName[i];
                var next = typeName[i + 1];

                if (char.IsUpper(current) && char.IsLower(next))
                {
                    indexes.Add(i);
                }
            }

            indexes.Add(typeName.Length);

            // Split the name into parts
            for (var i = 0; i < indexes.Count - 1; i++)
            {
                var current = indexes[i];
                var next = indexes[i + 1];

                yield return typeName.Substring(current, next - current);
            }
        }

        internal static IEnumerable<string> GetCombination(IList<string> list)
        {
            var builder = new StringBuilder();
            var count = Math.Pow(2, list.Count);

            for (int i = 1; i <= count - 1; i++)
            {
                builder.Clear();

                string str = Convert.ToString(i, 2).PadLeft(list.Count, '0');
                for (int j = 0; j < str.Length; j++)
                {
                    if (str[j] == '1')
                    {
                        builder.Append(list[j]);
                    }
                }

                var value = builder.ToString();

                yield return char.ToLower(value[0]) + value.Substring(1);
            }
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "bdkjfdsf dskjfds jds")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "blablabla")]
        internal static IEnumerable<string> CreateNameProposalsForCtorParams(Document document, SyntaxToken currentToken)
        {
            if (currentToken.Parent.IsKind(SyntaxKind.Parameter) &&
                currentToken.Parent.Parent.IsKind(SyntaxKind.ParameterList) &&
                currentToken.Parent.Parent.Parent.IsKind(SyntaxKind.ConstructorDeclaration))
            {
                var ctorDeclaration = currentToken.Parent.Parent.Parent as ConstructorDeclarationSyntax;
                var typeDeclaration = ctorDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>();

                var properties = typeDeclaration.Members.Where(m => m.IsKind(SyntaxKind.PropertyDeclaration)).Cast<PropertyDeclarationSyntax>();

                SemanticModel semanticModel = null;
                if (document.SupportsSemanticModel)
                {
                    document.TryGetSemanticModel(out semanticModel);
                }

                // this needs a good fix. The semantic model is rarely available here.
                // there's a lot of code in roslyn to handle the icompletionsource interface async,
                // this is a good reference to start. For new we should more results that the user
                // has to filter by typing some more characters. At the end this still saves time.
                if (semanticModel != null)
                {
                    var parameterType = semanticModel.GetTypeInfo(((ParameterSyntax)currentToken.Parent).Type).Type;

                    return properties
                        .Where(p => semanticModel.GetTypeInfo(p.Type).Type.Equals(parameterType))
                        .Select(p =>
                        char.ToLower(p.Identifier.ValueText[0], CultureInfo.CurrentCulture) + p.Identifier.ValueText.Substring(1));
                }
                else
                {
                    return properties
                        .Select(p =>
                            char.ToLower(p.Identifier.ValueText[0], CultureInfo.CurrentCulture) + p.Identifier.ValueText.Substring(1));
                }
            }

            return Enumerable.Empty<string>();
        }
    }
}
