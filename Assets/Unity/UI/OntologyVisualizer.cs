using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
                case CommonNoun c:
                    foreach (var parent in c.Superkinds)
                        yield return (c, parent, "kind of", null);
                    foreach (var child in c.Subkinds)
                        yield return (child, c, "kind of", null);
                    foreach (var a in c.RelevantAdjectives)
                        yield return (c, a, "can be", null);
                    foreach (var s in c.AlternativeSets)
                        foreach (var a in s.Alternatives)
                            yield return (c, a, "can be", null);
                    foreach (var a in c.ImpliedAdjectives)
                        if (a.Conditions.Length==0)
                            yield return (c, a.Adjective, "is always", null);
                        else
                            yield return (c, a.Adjective, "can be", null);
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

        g.GenerateFrom(CommonNoun.AllCommonNouns.Cast<Referent>().Concat(Verb.AllVerbs),
            o => (o.ToString(),
                (o is CommonNoun)?nounStyle:((o is Verb)?verbStyle:adjectiveStyle)), 
            Edges);
    }
}