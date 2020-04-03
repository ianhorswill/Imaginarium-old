#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Adjective.cs" company="Ian Horswill">
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

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A monadic predicate that is surfaced in English as an adjective.
/// </summary>
public class Adjective : MonadicConcept
{
    static Adjective()
    {
        Ontology.AllReferentTables.Add(AllAdjectives);
    }

    public Adjective(string[] name) : base(name)
    {
        Name = name;
        AllAdjectives[name] = this;
        Store(name, this);
        Driver.AppendResponseLine($"Learned the adjective <b><i>{name.Untokenize()}</i></b>.");
    }

    /// <summary>
    /// True if this is an adjective that can apply to an individual of the specified kind.
    /// </summary>
    /// <param name="noun">Noun representing a kind of object</param>
    /// <returns>True if this adjective is allowed to apply to objects of the specified kind.</returns>
    public bool RelevantTo(CommonNoun noun)
    {
        if (noun.RelevantAdjectives.Contains(this))
            return true;
        return noun.Superkinds.Any(RelevantTo);
    }

    private static readonly Dictionary<TokenString, Adjective> AllAdjectives = new Dictionary<TokenString, Adjective>();

    /// <summary>
    /// Returns the adjective with the specified name, or null if none
    /// </summary>
    public static Adjective Find(params string[] tokens) => AllAdjectives.LookupOrDefault(tokens);

    /// <summary>
    /// Token(s) that identify the adjective
    /// </summary>
    public readonly string[] Name;

    /// <inheritdoc />
    public override string[] StandardName => Name;

    /// <summary>
    /// Suppress this adjective during text generation.
    /// </summary>
    public bool IsSilent { get; set; }

    /// <inheritdoc />
    public override bool IsNamed(string[] tokens) => Name.SameAs(tokens);
}
