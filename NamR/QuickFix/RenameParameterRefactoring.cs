// <copyright file="RenameParameterRefactoring.cs" company="Ralf 'bexxx' Beckers">
// Copyright (c) Ralf 'bexxx' Beckers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace NamR.QuickFix
{
    using System;
    using System.Composition;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeRefactorings;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Rename;
    using Microsoft.CodeAnalysis.Text;

    [Shared]
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(RenameParameterRefactoring))]
    internal class RenameParameterRefactoring : CodeRefactoringProvider
    {
        public static async Task<Solution> RenameParameter(Document document, SyntaxToken token, string newName, CancellationToken cancellationToken)
        {
            return await Renamer.RenameSymbolAsync(
                document.Project.Solution,
                (await document.GetSemanticModelAsync(cancellationToken)).GetDeclaredSymbol(token.Parent),
                newName,
                document.Project.Solution.Workspace.Options,
                cancellationToken);
        }

        public sealed override Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            SyntaxToken currentToken = default(SyntaxToken);
            SyntaxNode root;
            if (!context.Document.TryGetSyntaxRoot(out root))
            {
                root = null;
            }

            currentToken = root.FindToken(context.Span.Start);

            string typeName = null;
            if (currentToken.Parent is ParameterSyntax && ((ParameterSyntax)currentToken.Parent).Identifier == currentToken)
            {
                typeName = ((IdentifierNameSyntax)((ParameterSyntax)currentToken.Parent).Type).Identifier.ValueText;
            }

            if (typeName != null)
            {
                var proposedNames = NamingHelper.CreateNameProposals(typeName).Where(n => n != currentToken.ValueText);

                foreach (var proposedName in proposedNames)
                {
                    context.RegisterRefactoring(
                        CodeAction.Create(
                            string.Format(CultureInfo.InvariantCulture, "Rename parameter to {0}", proposedName),
                            ct => RenameParameter(context.Document, currentToken, proposedName, ct)));
                }
            }

            return Task.CompletedTask;
        }
    }
}