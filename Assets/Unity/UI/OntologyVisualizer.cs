#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OntologyVisualizer.cs" company="Ian Horswill">
// Copyright (C) 2019, 2020 Ian Horswill
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using System.Linq;
using GraphVisualization;
using Imaginarium.Ontology;
using UnityEngine;

public class OntologyVisualizer : MonoBehaviour, IGraphGenerator
{
    public void GenerateGraph(Graph g)
    {
        var nounStyle = g.NodeStyleNamed("Noun");
        var adjectiveStyle = g.NodeStyleNamed("Adjective");
        var verbStyle = g.NodeStyleNamed("Verb");
        var errorStyle = g.NodeStyleNamed("Error");

        IEnumerable<(object from, object to, string label, EdgeStyle style)> Edges(object o)
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
                            yield return (c, a.Modifier.Concept, a.Modifier.IsPositive?"can be":"can be not", null);
                    foreach (var p in c.Parts)
                        yield return (c, p, "has part", null);
                    foreach (var prop in c.Properties)
                        yield return (c, prop, "has property", null);
                    break;

                case Verb v:
                    if (v.SubjectKind != null)
                        yield return (v.SubjectKind, v, "subject", null);
                    if (v.ObjectKind != null)
                        yield return (v, v.ObjectKind, "object", null);
                    foreach (var super in v.Generalizations)
                        yield return (v, super, "implies", null);
                    foreach (var super in v.Superspecies)
                        yield return (v, super, "is a way of", null);
                    foreach (var m in v.MutualExclusions)
                        yield return (v, m, "mutually exclusive", null);
                    break;

                case Part part:
                    yield return (part, part.Kind, "is a", null);
                    break;
            }
        }

        (string label, NodeStyle style) NodeLabel(object node)
        {
            switch (node)
            {
                //case Individual i:
                //    var iName = UIDriver.Invention?.NameString(i) ?? i.ToString();
                //    return (iName, nounStyle);
                case Noun n:
                    return (n.ToString(), nounStyle);

                case Verb v:
                    return (v.ToString(), (v.ObjectKind == null || v.SubjectKind == null)?errorStyle:verbStyle);

                default:
                    return (node.ToString(), adjectiveStyle);
            }
        }

        var nouns = UIDriver.Ontology.AllNouns.Select(pair => pair.Value).Cast<object>().ToArray();
        var verbs = UIDriver.Ontology.AllVerbs.ToArray();
        var vocabulary = nouns.Concat(verbs);
        //var ephemera = Generator.Current?.EphemeralIndividuals;

        g.GenerateFrom(vocabulary, NodeLabel, Edges);
    }
}