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

    public Adjective(string[] name)
    {
        Name = name;
        AllAdjectives[name] = this;
        Store(name, this);
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

    /// <inheritdoc />
    public override bool IsNamed(string[] tokens) => Name.SameAs(tokens);
}
