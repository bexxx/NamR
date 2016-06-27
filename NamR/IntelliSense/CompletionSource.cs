// <copyright file="CompletionSource.cs" company="Ralf 'bexxx' Beckers">
// Copyright (c) Ralf 'bexxx' Beckers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace NamR
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Media.Imaging;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Operations;

    internal class CompletionSource : ICompletionSource
    {
        private readonly CompletionSourceProvider sourceProvider;
        private readonly ITextBuffer textBuffer;
        ////private readonly SyntaxNode root;
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
            try
            {
                var workspace = session.TextView.TextBuffer.GetWorkspace();
                var docId = workspace.GetDocumentIdInCurrentContext(session.TextView.TextBuffer.AsTextContainer());
                var doc = workspace.CurrentSolution.GetDocument(docId);
                SyntaxNode root;
                if (!doc.TryGetSyntaxRoot(out root))
                {
                    root = null;
                }

                currentToken = root.FindToken(currentPoint.Position);
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }

            List<string> strList = null;
            if (currentToken.Parent is ParameterSyntax && ((ParameterSyntax)currentToken.Parent).Identifier == currentToken)
            {
                var typeName = ((IdentifierNameSyntax)((ParameterSyntax)currentToken.Parent).Type).Identifier.ValueText;
                var proposedNames = NamingHelper.CreateNameProposals(typeName);
                strList = new List<string>(proposedNames);
            }

            if (strList != null && strList.Count > 0)
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
