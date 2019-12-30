using System.Collections.Generic;
using System.Linq;
using GraphVisualization;
using UnityEngine;

public class OntologyVisualizer : MonoBehaviour, IGraphGenerator
{
    public void GenerateGraph(GraphVisualization.Graph g)
    {
        var nounStyle = g.NodeStyleNamed("Noun");
        var adjectiveStyle = g.NodeStyleNamed("Adjective");
        var verbStyle = g.NodeStyleNamed("Verb");

        IEnumerable<(object, object, string, EdgeStyle)> Edges(object o)
        {
            switch (o)
            {
                //case Individual i:
                //    if (UIDriver.Invention != null)
                //        foreach (var kind in UIDriver.Invention.MostSpecificNouns(i))
                //            yield return (i, kind, "is a", null);
                //    break;

                case ProperNoun p:
                    foreach (var kind in p.Kinds)
                        yield return (p, kind, "is a", null);
                    break;

                case CommonNoun c:
                    foreach (var parent in c.Superkinds)
                        yield return (c, parent, "kind of", null);
                    foreach (var child in c.Subkinds)
                        yield return (child, c, "kind of", null);
                    foreach (var a in c.RelevantAdjectives)
                        yield return (c, a, "can be", null);
                    foreach (var s in c.AlternativeSets)
                        foreach (var a in s.Alternatives)
                            yield return (c, a.Concept, "can be", null);
                    foreach (var a in c.ImpliedAdjectives)
                        if (a.Conditions.Length==0)
                            yield return (c, a.Modifier.Concept, a.Modifier.IsPositive?"is always":"is never", null);
                        else
                            yield return (c, a.Modifier, "can be", null);
                    break;

                case Verb v:
                    yield return (v.SubjectKind, v, "subject", null);
                    yield return (v, v.ObjectKind, "object", null);
                    foreach (var super in v.Generalizations)
                        yield return (v, super, "implies", null);
                    foreach (var m in v.MutualExclusions)
                        yield return (v, m, "mutually exclusive", null);
                    break;
            }
        }

        (string, NodeStyle) NodeLabel(object node)
        {
            switch (node)
            {
                //case Individual i:
                //    var iName = UIDriver.Invention?.NameString(i) ?? i.ToString();
                //    return (iName, nounStyle);
                case Noun n:
                    return (n.ToString(), nounStyle);

                case Verb v:
                    return (v.ToString(), verbStyle);

                default:
                    return (node.ToString(), adjectiveStyle);
            }
        }

        var nouns = Noun.AllNouns.Select(pair => pair.Value).Cast<object>();
        var verbs = Verb.AllVerbs;
        var vocabulary = nouns.Concat(verbs);
        //var ephemera = Generator.Current?.EphemeralIndividuals;

        g.GenerateFrom(vocabulary, NodeLabel, Edges);
    }
}