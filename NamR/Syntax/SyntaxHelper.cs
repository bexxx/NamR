// <copyright file="SyntaxHelper.cs" company="Ralf 'bexxx' Beckers">
// Copyright (c) Ralf 'bexxx' Beckers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace NamR
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class SyntaxHelper
    {
        public static TypeSyntax GetTypeSyntaxForToken(SyntaxToken token)
        {
            if (token.Parent is ParameterSyntax && ((ParameterSyntax)token.Parent).Identifier == token)
            {
                return ((ParameterSyntax)token.Parent).Type;
            }
            else if (
                token.Parent is VariableDeclaratorSyntax &&
                ((VariableDeclaratorSyntax)token.Parent).Identifier == token &&
                token.Parent.Parent is VariableDeclarationSyntax &&
                !((VariableDeclarationSyntax)token.Parent.Parent).Type.IsVar)
            {
                return ((VariableDeclarationSyntax)token.Parent.Parent).Type;
            }

            return null;
        }

        public static string GetNameFromTypeSyntax(TypeSyntax typeSyntax)
        {
            if (typeSyntax == null)
            {
                return null;
            }

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
            else if (typeSyntax.IsKind(SyntaxKind.GenericName))
            {
                var genericType = (GenericNameSyntax)typeSyntax;
                if (genericType.Arity == 1)
                {
                    return GetNameFromTypeSyntax(
                        genericType.TypeArgumentList.Arguments.FirstOrDefault());
                }
            }

            return null;
        }

        public static bool IsMultiple(TypeSyntax typeSyntax)
        {
            if (typeSyntax == null)
            {
                return false;
            }

            if (typeSyntax.IsKind(SyntaxKind.GenericName))
            {
                var identifier = ((GenericNameSyntax)typeSyntax).Identifier.ValueText;

                // yeah, this is quite a hack, but lights up the functionality nicely
                // to try it out.
                // TODO: use semantic model to check if this type implements IEnumerable<T>
                return identifier == "IEnumerable";
            }

            return false;
        }

        public static bool IsUppercase(SyntaxToken syntaxToken)
        {
            return syntaxToken.Parent.Parent.Parent is PropertyDeclarationSyntax ||
                               (syntaxToken.Parent.Parent.Parent is FieldDeclarationSyntax &&
                               ((FieldDeclarationSyntax)syntaxToken.Parent.Parent.Parent).Modifiers.Any(t => t.IsKind(SyntaxKind.PublicKeyword) || t.IsKind(SyntaxKind.ProtectedKeyword) || t.IsKind(SyntaxKind.InternalKeyword)));
        }
    }
}
