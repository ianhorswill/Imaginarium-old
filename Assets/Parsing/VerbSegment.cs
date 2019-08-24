using System;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using static Parser;

/// <summary>
/// A Segment representing a Noun (CommonNoun or ProperNoun)
/// </summary>
[DebuggerDisplay("VP \"{" + nameof(DebugText) + "}\"")]
public class VerbSegment : ReferringExpression<Verb>
{
    /// <summary>
    /// The Verb this NP refers to.
    /// </summary>
    public Verb Verb => Concept;

    private string DebugText => Text.Untokenize();

    #region Scanning
    /// <summary>
    /// Scan forward to the next occurence of token.
    /// </summary>
    /// <param name="token">Token that marks the end of this verb</param>
    /// <returns>True if token found and it marks a non-empty verb.</returns>
    public override bool ScanTo(string token)
    {
        if (ScanExistingVerb())
        {
            if (CurrentToken == token)
                return true;
        } else if (base.ScanTo(token))
            return true;
        return false;
    }

    /// <summary>
    /// Scan forward to the first token satisfying endPredicate.
    /// </summary>
    /// <param name="endPredicate">Predicate to test for the end of the NP</param>
    /// <returns>True if ending token found and it marks a non-empty NP.</returns>
    public override bool ScanTo(Func<string, bool> endPredicate)
    {
        if (ScanExistingVerb())
        {
            if (endPredicate(CurrentToken))
                return true;
        } else if (base.ScanTo(endPredicate))
            return true;
        return false;
    }

    /// <summary>
    /// Scan forward to the end of the input
    /// </summary>
    /// <param name="failOnConjunction">Must always be true - verbs with embedded conjunctions are not supported</param>
    /// <returns>True if successful</returns>
    public override bool ScanToEnd(bool failOnConjunction = true)
    {
        if (ScanExistingVerb())
        {
            if (EndOfInput)
                return true;
        } else if (base.ScanToEnd())
            return true;
        return false;
    }

    /// <summary>
    /// Attempt to match tokens to a known verb.
    /// </summary>
    /// <returns>True on success</returns>
    private bool ScanExistingVerb()
    {
        var old = State;
        CachedConcept = MatchTrie(Verb.Trie);
        if (CachedConcept == null)
        {
            ResetTo(old);
            return false;
        }

        Syntax.VerbNumber = Verb.Trie.LastMatchPlural ? Syntax.Number.Plural : Syntax.Number.Singular;
        return true;
    }
    #endregion

    protected override Verb GetConcept()
    {
        var text = Text;

        var verb = new Verb();

        if (Syntax.VerbNumber == Syntax.Number.Singular)
            verb.SingularForm = text;
        else
            // Note: this guarantees there is a singular form.
            verb.PluralForm = text;

        MaybeLoadDefinitions(verb);

        return verb;
    }
}