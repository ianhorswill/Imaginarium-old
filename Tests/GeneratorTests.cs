using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Parser;

namespace Tests
{
    [TestClass]
    public class GeneratorTests
    {
        [TestMethod]
        public void CatTest()
        {
            Ontology.EraseConcepts();
            ParseAndExecute("a cat is a kind of person",
                    "a persian is a kind of cat",
                    "a tabby is a kind of cat",
                    "a siamese is a kind of cat",
                    "a cat can be haughty",
                    "a cat can be cuddly",
                    "a cat can be crazy",
                    "a persian can be matted");
            var cat = (CommonNoun)Noun.Find("cat");
            var g = new Generator(cat);
            for (var n = 0; n < 100; n++)
            {
                var i = g.Solve();
                Assert.IsTrue(i.IsA(i.Individuals[0], cat));
                Assert.IsTrue(i.IsA(i.Individuals[0], "persian")
                              || i.IsA(i.Individuals[0], "tabby")
                              || i.IsA(i.Individuals[0], "siamese"));
                Console.WriteLine(i.Model.Model);
                Console.WriteLine(i.Description(i.Individuals[0]));
            }
        }

        [TestMethod]
        public void PartTest()
        {
            Ontology.EraseConcepts();
            ParseAndExecute("a cat is a kind of person",
                "a persian is a kind of cat",
                "a tabby is a kind of cat",
                "a siamese is a kind of cat",
                "a cat can be haughty",
                "a cat can be cuddly",
                "a cat can be crazy",
                "a persian can be matted",
                "red, blue, and green are kinds of color",
                "a cat has a color called its favorite color");
            var cat = (CommonNoun)Noun.Find("cat");
            var color = (CommonNoun) Noun.Find("color");
            var g = new Generator(cat);
            for (var n = 0; n < 100; n++)
            {
                var i = g.Solve();
                Assert.IsTrue(i.IsA(i.Individuals[0], cat));
                Assert.IsTrue(i.IsA(i.Individuals[0], "persian")
                              || i.IsA(i.Individuals[0], "tabby")
                              || i.IsA(i.Individuals[0], "siamese"));
                Assert.AreEqual(i.Individuals[0], i.Individuals[1].Container);
                Assert.IsTrue(i.IsA(i.Individuals[1], color));
                Console.WriteLine(i.Model.Model);
                Console.WriteLine(i.Description(i.Individuals[0]));
            }
        }

        [TestMethod]
        public void  CompoundNounTest()
        {
            Ontology.EraseConcepts();
            ParseAndExecute("a cat is a kind of person",
                "a persian is a kind of cat",
                "a tabby is a kind of cat",
                "a siamese is a kind of cat",
                "a cat can be haughty",
                "a cat can be cuddly",
                "a cat can be crazy",
                "a persian can be matted",
                "thaumaturge and necromancer are kinds of magic user");
            var cat = (CommonNoun)Noun.Find("cat");
            var magicUser = (CommonNoun)Noun.Find("magic", "user");
            var g = new Generator(cat, magicUser);
            for (var n = 0; n < 100; n++)
            {
                var i = g.Solve();
                Assert.IsTrue(i.IsA(i.Individuals[0], cat));
                Assert.IsTrue(i.IsA(i.Individuals[0], "persian")
                              || i.IsA(i.Individuals[0], "tabby")
                              || i.IsA(i.Individuals[0], "siamese"));
                Console.WriteLine(i.Model.Model);
                Console.WriteLine(i.Description(i.Individuals[0]));
            }
        }

        [TestMethod]
        public void ImpliedAdjectiveTest()
        {
            Ontology.EraseConcepts();
            ParseAndExecute("cats are fuzzy");
            var cat = CommonNoun.Find("cat");
            var fuzzy = Adjective.Find("fuzzy");
            var g = new Generator(cat);
            for (var n = 0; n < 100; n++)
            {
                var i = g.Solve();
                Assert.IsTrue(i.IsA(i.Individuals[0], fuzzy));
            }
        }

        [TestMethod]
        public void NumericPropertyTest()
        {
            Ontology.EraseConcepts();
            ParseAndExecute("cats have an age between 1 and 20");
            var cat = CommonNoun.Find("cat");
            var age = cat.Properties[0];
            var g = new Generator(cat);
            for (var n = 0; n < 100; n++)
            {
                var i = g.Solve();
                var ageVar = i.Individuals[0].Properties[age];
                var ageValue = (float)i.Model[ageVar];
                Assert.IsTrue(ageValue >= 1 && ageValue <= 20);
            }
        }

        [TestMethod]
        public void ProperNameTest()
        {
            Ontology.EraseConcepts();
            ParseAndExecute("a cat is a kind of person",
                "a persian is a kind of cat",
                "a tabby is a kind of cat",
                "a siamese is a kind of cat",
                "a cat can be haughty",
                "a cat can be cuddly",
                "a cat can be crazy",
                "a persian can be matted",
                "thaumaturgy is a form of magic",
                "necromancy is a form of magic",
                "a magic user must practice one form of magic");
            var cat = (CommonNoun)Noun.Find("cat");
            var magicUser = (CommonNoun)Noun.Find("magic", "user");
            var thaumaturgy = Individual.AllPermanentIndividuals["thaumaturgy"];
            var necromancy = Individual.AllPermanentIndividuals["necromancy"];
            var g = new Generator(cat, magicUser);
            for (var n = 0; n < 100; n++)
            {
                var i = g.Solve();
                Assert.IsTrue(i.Holds("practices", i.Individuals[0], thaumaturgy)
                              || i.Holds("practices", i.Individuals[0], necromancy));
                Console.WriteLine(i.Model.Model);
                Console.WriteLine(i.Description(i.Individuals[0]));
            }
        }
    }
}
