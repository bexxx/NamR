// <copyright file="CompletionSource.cs" company="Ralf 'bexxx' Beckers">
// Copyright (c) Ralf 'bexxx' Beckers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace NamR
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Operations;

    internal class CompletionSource : ICompletionSource
    {
        private readonly CompletionSourceProvider sourceProvider;
        private readonly ITextBuffer textBuffer;
        private List<Completion> compList;
        private bool isDisposed;

        public CompletionSource(CompletionSourceProvider sourceProvider, ITextBuffer textBuffer)
        {
            this.sourceProvider = sourceProvider;
            this.textBuffer = textBuffer;
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                GC.SuppressFinalize(this);
                this.isDisposed = true;
            }
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (completionSets == null)
            {
                throw new ArgumentNullException(nameof(completionSets));
            }

            SnapshotPoint currentPoint = session.TextView.Caret.Position.BufferPosition - 1;

            SyntaxToken currentToken;
            Document document;
            try
            {
                var workspace = session.TextView.TextBuffer.GetWorkspace();

                // e.g. in text inputs of VS related to code, but not actually code
                // there is no workspace (github issue #1: edit breakpoint actions)
                if (workspace == null)
                {
                    return;
                }

                var docId = workspace.GetDocumentIdInCurrentContext(session.TextView.TextBuffer.AsTextContainer());
                document = workspace.CurrentSolution.GetDocument(docId);

                SyntaxNode root;
                if (!document.TryGetSyntaxRoot(out root))
                {
                    root = null;
                }

                if (root == null)
                {
                    return;
                }

                currentToken = root.FindToken(currentPoint.Position);
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }

            if (!SyntaxHelper.IsSupportedSyntax(currentToken))
            {
                return;
            }

            List<string> strList = new List<string>();

            var typeSyntax = SyntaxHelper.GetTypeSyntaxForToken(currentToken);
            string typeName = SyntaxHelper.GetNameFromTypeSyntax(typeSyntax);
            bool isUpperCase = SyntaxHelper.IsUppercase(currentToken);
            bool isMultiple = SyntaxHelper.IsMultiple(typeSyntax);

            var beginning = currentToken.ValueText;

            if (!string.IsNullOrEmpty(typeName))
            {
                var proposedNames = NamingHelper.CreateNameProposals(typeName, isUpperCase, isMultiple, beginning).Where(n => n != currentToken.ValueText);

                strList.AddRange(proposedNames);
            }

            strList.AddRange(NamingHelper.CreateNameProposalsForCtorParams(document, currentToken));

            strList.AddRange(NamingHelper.CreateNameProposalsForNonPublicFields(currentToken, strList.ToList()));

            if (strList.Any())
            {
                this.compList = new List<Completion>(strList.Count);
                foreach (string str in strList)
                {
                    this.compList.Add(new Completion(str, str, str, null, null));
                }

                // TODO: figure out how to use localized resource assemblies in VS.
                completionSets.Add(new CompletionSet(
                    "Naming",    // the non-localized title of the tab
                    "Naming",    // the display title of the tab
                    this.FindTokenSpanAtPosition(session),
                    this.compList,
                    null));
            }
        }

        private ITrackingSpan FindTokenSpanAtPosition(ICompletionSession session)
        {
            SnapshotPoint currentPoint = session.TextView.Caret.Position.BufferPosition - 1;
            ITextStructureNavigator navigator = this.sourceProvider.NavigatorService.GetTextStructureNavigator(this.textBuffer);
            TextExtent extent = navigator.GetExtentOfWord(currentPoint);

            return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
        }
    }
}
