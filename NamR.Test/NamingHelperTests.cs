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
            CollectionAssert.AreEquivalent(new string[0], NamingHelper.CreateNameProposals("T").ToList());
            CollectionAssert.AreEquivalent(new string[0], NamingHelper.CreateNameProposals("TT").ToList());
            CollectionAssert.AreEquivalent(new string[0], NamingHelper.CreateNameProposals("I").ToList());
            CollectionAssert.AreEquivalent(new string[0], NamingHelper.CreateNameProposals("IE").ToList());
        }

        [TestMethod]
        public void ProposeLowerCaseForType()
        {
            CollectionAssert.AreEquivalent(new[] { "task" }, NamingHelper.CreateNameProposals("Task").ToList());
        }

        [TestMethod]
        public void OmitIForInterfaces()
        {
            CollectionAssert.AreEquivalent(new[] { "enumerable" }, NamingHelper.CreateNameProposals("IEnumerable").ToList());
        }

        [TestMethod]
        public void AbbreviateWithUpperCharactersOnly()
        {
            CollectionAssert.AreEquivalent(new[] { "cancellationToken", "ct" }, NamingHelper.CreateNameProposals("CancellationToken").ToList());
            CollectionAssert.AreEquivalent(new[] { "cancellationTokenSource", "cts" }, NamingHelper.CreateNameProposals("CancellationTokenSource").ToList());
        }

        [TestMethod]
        public void AbbreviateWithUpperCharactersOnlyForInterfaces()
        {
            CollectionAssert.AreEquivalent(new[] { "textView", "tv" }, NamingHelper.CreateNameProposals("ITextView").ToList());
        }
    }
}
