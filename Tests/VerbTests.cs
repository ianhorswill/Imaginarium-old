using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using static Parser;

namespace Tests
{
    [TestClass]
    public class VerbTests
    {
        [TestMethod]
        public void AntiReflexive()
        {
            Ontology.EraseConcepts();
            ParseAndExecute("people can love other people");
            ParseAndExecute("people cannot love themselves");
            var v = Verb.Find("loves");
            var g = new Generator(CommonNoun.Find("person"), new MonadicConcept[0], 10);
            for (int n = 0; n < 100; n++)
            {
                var s = g.Solve();
                foreach (var i in s.Individuals) Assert.IsFalse(s.Holds(v, i, i));
            }
        }

        [TestMethod]
        public void Reflexive()
        {
            Ontology.EraseConcepts();
            ParseAndExecute("people must love themselves");
            var v = Verb.Find("loves");
            var g = new Generator(CommonNoun.Find("person"), new MonadicConcept[0], 10);
            for (int n = 0; n < 100; n++)
            {
                var s = g.Solve();
                foreach (var i in s.Individuals) Assert.IsTrue(s.Holds(v, i, i));
            }
        }

        [TestMethod]
        public void PartialFunction()
        {
            Ontology.EraseConcepts();
            ParseAndExecute("people can love one person");
            var v = Verb.Find("loves");
            var g = new Generator(CommonNoun.Find("person"), new MonadicConcept[0], 3);
            bool sawNonTotal = false;

            for (var n = 0; n < 300; n++)
            {
                var s = g.Solve();
                foreach (var i in s.Individuals)
                {
                    var count = s.Individuals.Count(i2 => s.Holds(v, i, i2));
                    Assert.IsFalse(count > 1);
                    sawNonTotal |= count == 0;
                }
            }
            Assert.IsTrue(sawNonTotal);
        }

        [TestMethod]
        public void TotalFunction()
        {
            Ontology.EraseConcepts();
            ParseAndExecute("people must love one person");
            var v = Verb.Find("loves");
            var g = new Generator(CommonNoun.Find("person"), new MonadicConcept[0], 10);

            for (var n = 0; n < 100; n++)
            {
                var s = g.Solve();
                foreach (var i in s.Individuals)
                {
                    var count = s.Individuals.Count(i2 => s.Holds(v, i, i2));
                    Assert.IsTrue(count == 1);
                }
            }
        }

        [TestMethod]
        public void Symmetric()
        {
            Ontology.EraseConcepts();
            ParseAndExecute("people can love each other");
            var v = Verb.Find("loves");
            var g = new Generator(CommonNoun.Find("person"), new MonadicConcept[0], 10);

            foreach (var i1 in g.Individuals)
            foreach (var i2 in g.Individuals)
                Assert.IsTrue(ReferenceEquals(g.Holds(v, i1, i2), g.Holds(v, i2, i1)));
        }
    }
}
