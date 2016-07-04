// <copyright file="SyntaxHelper.cs" company="Ralf 'bexxx' Beckers">
// Copyright (c) Ralf 'bexxx' Beckers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace NamR
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class SyntaxHelper
    {
        public static string GetNameFromTypeSyntax(TypeSyntax typeSyntax)
        {
            if (typeSyntax.IsKind(SyntaxKind.IdentifierName))
            {
                return ((IdentifierNameSyntax)typeSyntax).Identifier.ValueText;
            }
            else if (typeSyntax.IsKind(SyntaxKind.NullableType))
            {
                return GetNameFromTypeSyntax(((NullableTypeSyntax)typeSyntax).ElementType);
            }
            else if (typeSyntax.IsKind(SyntaxKind.QualifiedName))
            {
                return ((QualifiedNameSyntax)typeSyntax).Right.Identifier.ValueText;
            }

            return null;
        }
    }
}
