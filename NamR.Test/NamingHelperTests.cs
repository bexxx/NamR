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
                CollectionAssert.AreEquivalent(new string[0], NamingHelper.CreateNameProposals("T", false, casing).ToList());
                CollectionAssert.AreEquivalent(new string[0], NamingHelper.CreateNameProposals("TT", false, casing).ToList());
                CollectionAssert.AreEquivalent(new string[0], NamingHelper.CreateNameProposals("I", false, casing).ToList());
                CollectionAssert.AreEquivalent(new string[0], NamingHelper.CreateNameProposals("IE", false, casing).ToList());
            }
        }

        [TestMethod]
        public void ProposeLowerCaseForType()
        {
            CollectionAssert.AreEquivalent(new[] { "task" }, NamingHelper.CreateNameProposals("Task", false, false).ToList());
            CollectionAssert.AreEquivalent(new[] { "Task" }, NamingHelper.CreateNameProposals("Task", true, false).ToList());
            CollectionAssert.AreEquivalent(new[] { "tasks" }, NamingHelper.CreateNameProposals("Task", false, true).ToList());
            CollectionAssert.AreEquivalent(new[] { "Tasks" }, NamingHelper.CreateNameProposals("Task", true, true).ToList());
        }

        [TestMethod]
        public void OmitIForInterfaces()
        {
            CollectionAssert.AreEquivalent(new[] { "enumerable" }, NamingHelper.CreateNameProposals("IEnumerable", false, false).ToList());
            CollectionAssert.AreEquivalent(new[] { "Enumerable" }, NamingHelper.CreateNameProposals("IEnumerable", true, false).ToList());
        }

        [TestMethod]
        public void AbbreviateWithUpperCharactersOnly()
        {
            CollectionAssert.AreEquivalent(new[] { "cancellationToken", "ct" }, NamingHelper.CreateNameProposals("CancellationToken", false, false).ToList());
            CollectionAssert.AreEquivalent(new[] { "cancellationTokenSource", "cts" }, NamingHelper.CreateNameProposals("CancellationTokenSource", false, false).ToList());
            CollectionAssert.AreEquivalent(new[] { "CancellationToken", "Ct" }, NamingHelper.CreateNameProposals("CancellationToken", true, false).ToList());
            CollectionAssert.AreEquivalent(new[] { "CancellationTokenSource", "Cts" }, NamingHelper.CreateNameProposals("CancellationTokenSource", true, false).ToList());
        }

        [TestMethod]
        public void AbbreviateWithUpperCharactersOnlyForInterfaces()
        {
            CollectionAssert.AreEquivalent(new[] { "textView", "tv" }, NamingHelper.CreateNameProposals("ITextView", false, false).ToList());
            CollectionAssert.AreEquivalent(new[] { "TextView", "Tv" }, NamingHelper.CreateNameProposals("ITextView", true, false).ToList());
        }

        [TestMethod]
        public void ProposeCommonNames()
        {
            CollectionAssert.AreEquivalent(new[] { "id" }, NamingHelper.CreateNameProposals("Guid", false, false).ToList());
            CollectionAssert.AreEquivalent(new[] { "ids" }, NamingHelper.CreateNameProposals("Guid", false, true).ToList());
        }

        [TestMethod]
        public void AppendCommonSuffix()
        {
            CollectionAssert.AreEquivalent(new[] { "id", "FooId" }, NamingHelper.CreateNameProposals("Guid", false, false, "Foo").ToList());
        }

        [TestMethod]
        public void AppendMultipleCommonSuffix()
        {
            CollectionAssert.AreEquivalent(new[] { "FooLength", "FooCount" }, NamingHelper.CreateNameProposals("int", false, false, "Foo").ToList());
        }
    }
}
