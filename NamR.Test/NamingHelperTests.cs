namespace NamR.Test
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NamingHelperTests
    {
        [TestMethod]
        public void MustBeTwoCharactersMinimum()
        {
            foreach (var casing in new[] { true, false })
            {
                CollectionAssert.AreEquivalent(new string[0], NamingHelper.CreateNameProposals("T", casing).ToList());
                CollectionAssert.AreEquivalent(new string[0], NamingHelper.CreateNameProposals("TT", casing).ToList());
                CollectionAssert.AreEquivalent(new string[0], NamingHelper.CreateNameProposals("I", casing).ToList());
                CollectionAssert.AreEquivalent(new string[0], NamingHelper.CreateNameProposals("IE", casing).ToList());
            }
        }

        [TestMethod]
        public void ProposeLowerCaseForType()
        {
            CollectionAssert.AreEquivalent(new[] { "task" }, NamingHelper.CreateNameProposals("Task", false).ToList());
            CollectionAssert.AreEquivalent(new[] { "Task" }, NamingHelper.CreateNameProposals("Task", true).ToList());
        }

        [TestMethod]
        public void OmitIForInterfaces()
        {
            CollectionAssert.AreEquivalent(new[] { "enumerable" }, NamingHelper.CreateNameProposals("IEnumerable", false).ToList());
            CollectionAssert.AreEquivalent(new[] { "Enumerable" }, NamingHelper.CreateNameProposals("IEnumerable", true).ToList());
        }

        [TestMethod]
        public void AbbreviateWithUpperCharactersOnly()
        {
            CollectionAssert.AreEquivalent(new[] { "cancellationToken", "ct" }, NamingHelper.CreateNameProposals("CancellationToken", false).ToList());
            CollectionAssert.AreEquivalent(new[] { "cancellationTokenSource", "cts" }, NamingHelper.CreateNameProposals("CancellationTokenSource", false).ToList());
            CollectionAssert.AreEquivalent(new[] { "CancellationToken", "Ct" }, NamingHelper.CreateNameProposals("CancellationToken", true).ToList());
            CollectionAssert.AreEquivalent(new[] { "CancellationTokenSource", "Cts" }, NamingHelper.CreateNameProposals("CancellationTokenSource", true).ToList());
        }

        [TestMethod]
        public void AbbreviateWithUpperCharactersOnlyForInterfaces()
        {
            CollectionAssert.AreEquivalent(new[] { "textView", "tv" }, NamingHelper.CreateNameProposals("ITextView", false).ToList());
            CollectionAssert.AreEquivalent(new[] { "TextView", "Tv" }, NamingHelper.CreateNameProposals("ITextView", true).ToList());
        }

        [TestMethod]
        public void ProposeCommonNames()
        {
            CollectionAssert.AreEquivalent(new[] { "id", "guid" }, NamingHelper.CreateNameProposals("Guid", false).ToList());
        }
    }
}
