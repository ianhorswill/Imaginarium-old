using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Operations for accessing the ontology as a whole
/// The ontology consists of all the Referent objects and the information within them (e.g. Property objects)
/// </summary>
public static class Ontology
{
    /// <summary>
    /// List of all the tables of different kinds of referents.
    /// Used so we know what to clear when reinitializing the ontology.
    /// </summary>
    public static readonly List<IDictionary> AllReferentTables = new List<IDictionary>();

    /// <summary>
    /// Removes all concepts form the ontology.
    /// </summary>
    public static void EraseConcepts()
    {
        foreach (var c in AllReferentTables)
            c.Clear();
        
        TokenTrieBase.ClearAllTries();
    }
}
