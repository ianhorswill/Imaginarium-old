using System.Collections.Generic;

/// <summary>
/// A concept that represents a unary predicate, e.g. a noun or adjective.
/// </summary>
public abstract class MonadicConcept : Concept
{
    public static readonly TokenTrie<MonadicConcept> Trie = new TokenTrie<MonadicConcept>();
    public static bool LastMatchPlural => Trie.LastMatchPlural;

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