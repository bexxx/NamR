// <copyright file="CompletionCommandHandler.cs" company="Ralf 'bexxx' Beckers">
// Copyright (c) Ralf 'bexxx' Beckers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace NamR
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Language.Intellisense;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.TextManager.Interop;

    internal class CompletionCommandHandler : IOleCommandTarget
    {
        private IOleCommandTarget nextCommandHandler;
        private ITextView textView;
        private CompletionHandlerProvider provider;
        private ICompletionSession session;

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.TextManager.Interop.IVsTextView.AddCommandFilter(Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget,Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget@)", Justification = "If it fails, it fails :).")]
        internal CompletionCommandHandler(IVsTextView textViewAdapter, ITextView textView, CompletionHandlerProvider provider)
        {
            this.textView = textView;
            this.provider = provider;

            // add the command to the command chain
            textViewAdapter.AddCommandFilter(this, out this.nextCommandHandler);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return this.nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (VsShellUtilities.IsInAutomationFunction(this.provider.ServiceProvider))
            {
                return this.nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }

            // make a copy of this so we can look at it after forwarding some commands
            uint commandID = nCmdID;
            char typedChar = char.MinValue;

            // make sure the input is a char before getting it
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            }

            // check for a commit character
            if (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN
                || nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB
                || (char.IsWhiteSpace(typedChar) || char.IsPunctuation(typedChar)))
            {
                // check for a a selection
                if (this.session != null && !this.session.IsDismissed)
                {
                    // if the selection is fully selected, commit the current session
                    if (this.session.SelectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        this.session.Commit();

                        // also, don't add the character to the buffer
                        return VSConstants.S_OK;
                    }
                    else
                    {
                        // if there is no selection, dismiss the session
                        this.session.Dismiss();
                    }
                }
            }

            // pass along the command so the char is added to the buffer
            int retVal = this.nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            bool handled = false;
            if (!typedChar.Equals(char.MinValue) && char.IsLetterOrDigit(typedChar))
            {
                if (this.session == null || this.session.IsDismissed)
                {
                    // If there is no active session, bring up completion
                    this.TriggerCompletion();
                }
                else
                {
                    // the completion session is already active, so just filter
                    this.session.Filter();
                }

                handled = true;
            }
            else if (commandID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE
                || commandID == (uint)VSConstants.VSStd2KCmdID.DELETE)
            {
                // redo the filter if there is a deletion
                if (this.session != null && !this.session.IsDismissed)
                {
                    this.session.Filter();
                }

                handled = true;
            }

            if (handled)
            {
                return VSConstants.S_OK;
            }

            return retVal;
        }

        private bool TriggerCompletion()
        {
            // the caret must be in a non-projection location
            SnapshotPoint? caretPoint =
            this.textView.Caret.Position.Point.GetPoint(
            textBuffer => (!textBuffer.ContentType.IsOfType("projection")), PositionAffinity.Predecessor);
            if (!caretPoint.HasValue)
            {
                return false;
            }

            this.session = this.provider.CompletionBroker.CreateCompletionSession(
                this.textView,
                caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive),
                true);

            // subscribe to the Dismissed event on the session
            this.session.Dismissed += this.OnSessionDismissed;
            this.session.Start();

            return true;
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            this.session.Dismissed -= this.OnSessionDismissed;
            this.session = null;
        }
    }
}
