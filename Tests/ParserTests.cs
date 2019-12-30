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
            Ontology.EraseConcepts();
            ParseAndExecute("the plural of person is people");
            Assert.AreEqual("people",((CommonNoun)Noun.Find("person")).PluralForm[0]);
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
        public void APListTest()
        {
            Ontology.EraseConcepts();
            ParseAndExecute("Cats can be white, black, or ginger");
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
            Assert.IsNotNull(Noun.Find("Persian"));
            Assert.IsNotNull(Noun.Find("Persians"));
        }

        [TestMethod]
        public void ParseAntiReflexiveTest()
        {
            Ontology.EraseConcepts();
            ParseAndExecute("Cats are a kind of person.",
                "Cats cannot love themselves");
            var love = Verb.Find("love");
            Assert.IsNotNull(love);
        }

        [TestMethod]
        public void ParseReflexiveTest()
        {
            Ontology.EraseConcepts();
            ParseAndExecute("Cats are a kind of person.",
                "Cats must love themselves");
            var love = Verb.Find("love");
            Assert.IsNotNull(love);
        }
    }
}
