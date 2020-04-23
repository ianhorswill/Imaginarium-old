#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Individual.cs" company="Ian Horswill">
// Copyright (C) 2019 Ian Horswill
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

using System;
using System.Collections.Generic;
using System.Linq;
using CatSAT;

/// <summary>
/// Represents an object in the model (imagined scenario)
/// </summary>
public class Individual : Referent, IComparable
{
    /// <summary>
    /// Makes an Individual that is not part of the ontology itself.
    /// This individual is local to a particular Invention.
    /// </summary>
    /// <param name="concepts">CommonNouns and Adjectives that must apply to the individual</param>
    /// <param name="name">Default name to give to the individual if no name property can be found.</param>
    /// <param name="container">The object of which this is a part, if any</param>
    /// <returns></returns>
    public static Individual Ephemeral(IEnumerable<MonadicConceptLiteral> concepts, string[] name, Individual container = null)
    {
        return new Individual(concepts, name, container);
    }

    /// <summary>
    /// Makes an Individual that is part of the ontology itself.  It will appear in all Inventions.
    /// </summary>
    /// <param name="concepts">CommonNouns and Adjectives that must be true of this Individual</param>
    /// <param name="name">Default name for the individual if not name property can be found.</param>
    /// <returns></returns>
    public static Individual Permanent(IEnumerable<MonadicConceptLiteral> concepts, string[] name)
    {
        var individual = new Individual(concepts, name);
        AllPermanentIndividuals[name] = individual;
        return individual;
    }

    static Individual()
    {
        Ontology.AllReferentTables.Add(AllPermanentIndividuals);
    }

    /// <summary>
    /// Returns the Property of this Individual named "name", if any
    /// </summary>
    /// <returns>The name Property, else null.</returns>
    public Property NameProperty()
    {
        foreach (var pair in Properties)
            if (pair.Key.IsNamed(NameTokenString))
                return pair.Key;
        return null;
    }
    private static readonly string[] NameTokenString = new[] {"name"};

    /// <summary>
    /// The kinds (CommonNouns) of this Individual
    /// </summary>
    public readonly List<CommonNoun> Kinds = new List<CommonNoun>();

    /// <summary>
    /// The Adjectives that might apply to this Individual
    /// </summary>
    public readonly List<MonadicConceptLiteral> Modifiers = new List<MonadicConceptLiteral>();

    /// <summary>
    /// The Properties of this individual.
    /// </summary>
    public readonly Dictionary<Part, Individual> Parts = new Dictionary<Part, Individual>();
    
    /// <summary>
    /// The Properties of this individual.
    /// </summary>
    public readonly Dictionary<Property, Variable> Properties = new Dictionary<Property, Variable>();
    
    private Individual(IEnumerable<MonadicConceptLiteral> concepts, string[] name, Individual container = null) : base(name)
    {
        Name = name;
        Container = container;
        var enumerated = concepts as MonadicConceptLiteral[] ?? concepts.ToArray();
        Kinds.AddRange(enumerated.Where(l => l.IsPositive && l.Concept is CommonNoun).Select(l => (CommonNoun)l.Concept));
        Modifiers.AddRange(enumerated.Where(l => !l.IsPositive || !(l.Concept is CommonNoun)));
    }

    public static Dictionary<TokenString, Individual> AllPermanentIndividuals = new Dictionary<TokenString, Individual>();
    public static Individual Find(params string[] tokens) => AllPermanentIndividuals.LookupOrDefault(tokens);

    /// <summary>
    /// Name of the object within the ontology, for permanent individuals.
    /// Also used as a default name for ephemeral individuals, if they don't end up with any assigned
    /// name property.
    /// </summary>
    public readonly string[] Name;

    /// <summary>
    /// The Individual of which this is a part
    /// </summary>
    public readonly Individual Container;

    /// <inheritdoc />
    public override string[] StandardName => Name;

    public string MostRecentDescription { get; set; }

    public override bool IsNamed(string[] tokens) => Name.SameAs(tokens);

    // ReSharper disable once InconsistentNaming
    private static int UIDCounter;
    // ReSharper disable once InconsistentNaming
    private readonly int UID = UIDCounter++;

    public int CompareTo(object obj)
    {
        if (obj is Individual i)
            return UID.CompareTo(i.UID);
        return -1;
    }
}
