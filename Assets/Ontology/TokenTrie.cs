#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TokenTrie.cs" company="Ian Horswill">
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

public abstract class TokenTrieBase
{
    protected TokenTrieBase(Ontology ontology)
    {
        ontology.AllTokenTries.Add(this);
    }

    public abstract void Clear();

    public bool Contains(TokenString t) => Find(t) != null;

    public abstract object Find(TokenString t);

    public bool IsCaseSensitive = false;
}

/// <summary>
/// A trie mapping sequences of tokens to Referents
/// </summary>
/// <typeparam name="TReferent"></typeparam>
public class TokenTrie<TReferent> : TokenTrieBase
    where TReferent : Referent
{
    public TokenTrie(Ontology ontology) : base(ontology)
    { }

    private readonly Node root = new Node();

    public bool IsEmpty => root.Dict.Count == 0;

    public IEnumerable<TReferent> Contents => SubtreeContents(root);

    IEnumerable<TReferent> SubtreeContents(Node n)
    {
        var children = n.Dict.SelectMany(pair => SubtreeContents(pair.Value));
        return n.Concept != null ? children.Prepend(n.Concept) : children;
    }

    /// <summary>
    /// Add this name and concept to the trie.
    /// </summary>
    /// <param name="tokens">Name to add for the concept</param>
    /// <param name="c">Concept to add</param>
    /// <param name="isPlural">True when concept is a common noun and the name is its plural.</param>
    public void Store(string[] tokens, TReferent c, bool isPlural = false)
    {
        var node = root;
        foreach (var tok in tokens)
        {
            if (node.Concept != null)
                throw new GrammaticalError(
                    $"You tried to define a term, \"{tokens.Untokenize()}\", but it starts with the phrase for an existing term, \"{node.Concept.StandardName.Untokenize()}\".  Imaginarium doesn't allow this because it can create situations of ambiguity and Imaginarium doesn't handle ambiguity well",
                    $"You tried to define a term, \"<b><i>{tokens.Untokenize()}</i></b>\", but it starts with the phrase for an existing term, \"<b><i>{node.Concept.StandardName.Untokenize()}</i></b>\".  Imaginarium doesn't allow this because it can create situations of ambiguity and Imaginarium doesn't handle ambiguity well");

            var t = IsCaseSensitive?tok:tok.ToLower();
            if (node.Dict.TryGetValue(t, out Node match))
            {
                node = match;
            }
            else
            {
                var n = new Node();
                node.Dict[t] = n;
                node = n;
            }
        }

        if (node.Dict != null && node.Dict.Count > 0)
            throw new GrammaticalError(
                $"You tried to define a term, \"{tokens.Untokenize()}\", but you already have a another term that starts with those words.  Imaginarium doesn't allow this because it can create situations of ambiguity and Imaginarium doesn't handle ambiguity well",
                $"You tried to define a term, \"<b><i>{tokens.Untokenize()}</i></b>\", but you already have a another term that starts with those words.  Imaginarium doesn't allow this because it can create situations of ambiguity and Imaginarium doesn't handle ambiguity well");

        node.Concept = c;
        node.IsPlural = isPlural;
    }

    /// <summary>
    /// Returns information about the plurality of the last match, if relevant.
    /// </summary>
    public bool LastMatchPlural;

    /// <summary>
    /// Search trie for a monadic concept named by some substring of tokens starting at the specified index.
    /// Updates index as it searches
    /// </summary>
    /// <param name="tokens">Sequence of tokens to search</param>
    /// <param name="index">Position within token sequence</param>
    /// <returns>Concept, if found, otherwise null.</returns>
    public TReferent Lookup(IList<string> tokens, ref int index)
    {
        var node = root;
        while (index < tokens.Count)
        {
            var tok = tokens[index++];
            var t = IsCaseSensitive?tok:tok.ToLower();
            if (node.Dict.TryGetValue(t, out Node match))
            {
                node = match;
                if (node.Concept != null)
                {
                    LastMatchPlural = node.IsPlural;
                    return node.Concept;
                }
            }
            else
                return null;
        }

        return null;
    }

    public override object Find(TokenString tokens)
    {
        var node = root;
        foreach (var word in tokens.Tokens)
            if (!node.Dict.TryGetValue(word, out node))
                return null;

        return node.Concept;
    }

    /// <summary>
    /// Remove all data from the monadic concept trie
    /// Used when reinitializing the ontology.
    /// </summary>
    public override void Clear()
    {
        root.Dict.Clear();
    }

    private class Node
    {
        public readonly Dictionary<string, Node> Dict = new Dictionary<string, Node>();
        public TReferent Concept;

        public bool IsPlural;
    }
}