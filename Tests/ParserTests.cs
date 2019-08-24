using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Parser;

namespace Tests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod, ExpectedException(typeof(GrammaticalError))]
        public void GibberishTest()
        {
            ParseAndExecute("foo bar baz");
        }

        [TestMethod]
        public void PluralDeclarationTest()
        {
            ParseAndExecute("the plural of person is people");
            Assert.AreEqual("people",Noun.Find("person").PluralForm[0]);
        }

        [TestMethod]
        public void KindOfTestSingular()
        {
            ParseAndExecute("a cat is a kind of person");
            Assert.IsTrue(Syntax.Subject.CommonNoun.IsImmediateSubKindOf(Syntax.Object.CommonNoun));
        }

        [TestMethod]
        public void KindOfTestPlural()
        {
            ParseAndExecute("cats are a kind of person");
            Assert.IsTrue(Syntax.Subject.CommonNoun.IsImmediateSubKindOf(Syntax.Object.CommonNoun));
        }

        [TestMethod]
        public void AdjectiveDeclarationTestPlural()
        {
            ParseAndExecute("cats can be lovely");
            Assert.IsTrue(Syntax.PredicateAP.Adjective.RelevantTo(Syntax.Subject.CommonNoun));
        }

        [TestMethod]
        public void NPListTest()
        {
            ParseAndExecute("tabby, persian, and siamese are kinds of cat");
            Assert.IsTrue(CommonNoun.Find("tabby").IsImmediateSubKindOf(CommonNoun.Find("cat")));
            Assert.IsTrue(CommonNoun.Find("persian").IsImmediateSubKindOf(CommonNoun.Find("cat")));
            Assert.IsTrue(CommonNoun.Find("siamese").IsImmediateSubKindOf(CommonNoun.Find("cat")));
        }

        [TestMethod]
        public void RequiredAlternativeSetTest()
        {
            Ontology.EraseConcepts();
            ParseAndExecute("cats are long haired or short haired");
            var cat = CommonNoun.Find("cat");
            Assert.AreEqual(1, cat.AlternativeSets.Count);
        }

        [TestMethod]
        public void OptionalAlternativeSetTest()
        {
            Ontology.EraseConcepts();
            ParseAndExecute("cats can be big or small");
            var cat = CommonNoun.Find("cat");
            Assert.AreEqual(1, cat.AlternativeSets.Count);
        }

        [TestMethod]
        public void InterningNounTest()
        {
            Ontology.EraseConcepts();
            ParseAndExecute("Tabby, Persian, and Maine Coon are kinds of cat");
            Assert.IsNotNull(Noun.Find(new[] {"Persian"}));
            Assert.IsNotNull(Noun.Find(new[] {"Persians"}));
        }
    }
}
