#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MonadicConcept.cs" company="Ian Horswill">
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

/// <summary>
/// A concept that represents a unary predicate, e.g. a noun or adjective.
/// </summary>
public abstract class MonadicConcept : Concept
{
    public static readonly TokenTrie<MonadicConcept> Trie = new TokenTrie<MonadicConcept>();
    public static bool LastMatchPlural => Trie.LastMatchPlural;

    /// <summary>
    /// The initial probability used for the proposition that an individual is of this type.
    /// </summary>
    public float InitialProbability = 0.5f;

    /// <summary>
    /// Add this name and concept to the trie of all known names of all known monadic concepts.
    /// </summary>
    /// <param name="tokens">Name to add for the concept</param>
    /// <param name="c">Concept to add</param>
    /// <param name="isPlural">True when concept is a common noun and the name is its plural.</param>
    public static void Store(string[] tokens, MonadicConcept c, bool isPlural = false) => Trie.Store(tokens, c, isPlural);

    /// <summary>
    /// Search trie for a monadic concept named by some substring of tokens starting at the specified index.
    /// Updates index as it searches
    /// </summary>
    /// <param name="tokens">Sequence of tokens to search</param>
    /// <param name="index">Position within token sequence</param>
    /// <returns>Concept, if found, otherwise null.</returns>
    public static MonadicConcept Lookup(IList<string> tokens, ref int index) => Trie.Lookup(tokens, ref index);
}