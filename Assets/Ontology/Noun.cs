using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// A monadic concept that can be realized in English as the head of an NP.
/// </summary>
[DebuggerDisplay("{" + nameof(Text) + "}")]
public abstract class Noun : MonadicConcept
{
    static Noun()
    {
        Ontology.AllReferentTables.Add(AllNouns);
    }

    public static Dictionary<TokenString, Noun> AllNouns = new Dictionary<TokenString, Noun>();

    /// <summary>
    /// Returns the noun named by the specified token string, or null if there is none.
    /// </summary>
    public static Noun Find(params string[] tokens) => AllNouns.LookupOrDefault(tokens);


 }