using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Parser;

/// <summary>
/// A Segment representing a Noun (CommonNoun or ProperNoun)
/// </summary>
[DebuggerDisplay("NP \"{" + nameof(DebugText) + "}\"")]
public class NP : ReferringExpression<Noun>
{
    /// <summary>
    /// The Noun this NP refers to.
    /// </summary>
    public Noun Noun => Concept;

    /// <summary>
    /// The CommonNoun this NP refers to (or exception if it's a proper noun)
    /// </summary>
    public CommonNoun CommonNoun
    {
        get
        {
            var n = Noun as CommonNoun;
            if (n == null)
                throw new GrammaticalError("Not a common noun", Text);
            return n;
        }
    }

    /// <summary>
    /// The modifiers (adjectives or other common nouns) applied to the CommonNoun head, if any.
    /// For example, in "quick, brown fox", fox is the CommonNoun and quick and brown are modifiers.
    /// </summary>
    public List<MonadicConcept> Modifiers = new List<MonadicConcept>();

    /// <summary>
    /// True if the segment starts with a determiner
    /// </summary>
    private bool beginsWithDeterminer;
    /// <summary>
    /// True if we've been told by our syntax rule that this has to be a common noun.
    /// </summary>
    public bool ForceCommonNoun;

    #region Scanning
    /// <summary>
    /// Scan forward to the next occurence of token.
    /// </summary>
    /// <param name="token">Token that marks the end of this NP</param>
    /// <returns>True if token found and it marks a non-empty NP.</returns>
    public override bool ScanTo(string token)
    {
        UnityEngine.Debug.Assert(CachedConcept == null);
        var old = State;
        ScanDeterminer();
        if (ScanComplexNP())
        {
            if (CurrentToken == token)
                return true;
        } else if (base.ScanTo(token))
            return true;
        ResetTo(old);
        return false;
    }

    /// <summary>
    /// Scan forward to the first token satisfying endPredicate.
    /// </summary>
    /// <param name="endPredicate">Predicate to test for the end of the NP</param>
    /// <returns>True if ending token found and it marks a non-empty NP.</returns>
    public override bool ScanTo(Func<string, bool> endPredicate)
    {
        var old = State;
        ScanDeterminer();
        if (ScanComplexNP())
        {
            if (endPredicate(CurrentToken))
                return true;
        } else if (base.ScanTo(endPredicate))
            return true;
        ResetTo(old);
        return false;
    }

    /// <summary>
    /// Scan forward to the end of the input
    /// </summary>
    /// <param name="failOnConjunction">Must always be true - NPs with embedded conjunctions are not supported</param>
    /// <returns>True if successful</returns>
    public override bool ScanToEnd(bool failOnConjunction = true)
    {
        var old = State;
        ScanDeterminer();

        if (ScanComplexNP())
        {
            if (EndOfInput)
                return true;
        } else  if (base.ScanToEnd(failOnConjunction))
            return true;
        ResetTo(old);
        return false;
    }

    /// <summary>
    /// Skip over a determiner if we see one, and update state variables.
    /// </summary>
    private void ScanDeterminer()
    {
        beginsWithDeterminer = true;
        if (Match("a") || Match("an"))
            Number = Syntax.Number.Singular;
        else if (Match("all"))
            Number = Syntax.Number.Plural;
        else if (Match("one"))
            ExplicitCount = 1;
        else if (Match("two"))
            ExplicitCount = 2;
        else if (Match("three"))
            ExplicitCount = 3;
        else if (Match("four"))
            ExplicitCount = 4;
        else if (Match("five"))
            ExplicitCount = 5;
        else if (Match("six"))
            ExplicitCount = 6;
        else if (Match("seven"))
            ExplicitCount = 7;
        else if (Match("eight"))
            ExplicitCount = 8;
        else if (Match("nine"))
            ExplicitCount = 9;
        else if (Match("ten"))
            ExplicitCount = 10;
        else if (int.TryParse(CurrentToken, out int count))
        {
            ExplicitCount = count;
            SkipToken();
        }
        else
            beginsWithDeterminer = false;
    }

    /// <summary>
    /// Attempt to match tokens to a complex NP, including modifiers.
    /// If successful, this sets Modifiers and CommonNoun directly.
    /// Will fail phrase includes an unknown noun or adjective.
    /// </summary>
    /// <returns>True on success</returns>
    private bool ScanComplexNP()
    {
        UnityEngine.Debug.Assert(CachedConcept == null);
        var old = State;
        MonadicConcept next;
        MonadicConcept last = null;
        Modifiers.Clear();
        do
        {
            next = MatchTrie(MonadicConcept.Trie);
            if (next != null)
            {
                if (last != null)
                    Modifiers.Add(last);
                last = next;
                if (!EndOfInput && CurrentToken == ",")
                    SkipToken();
            }
        } while (next != null);

        if (last != null && last is Noun n)
        {
            CachedConcept = n;
            Number = MonadicConcept.LastMatchPlural ? Syntax.Number.Plural : Syntax.Number.Singular;
            return true;
        }

        ResetTo(old);
        return false;
    }
    #endregion

    /// <summary>
    /// Find the Noun this NP refers to.
    /// IMPORTANT:
    /// - This is called after scanning, so it's only called once we've verified there's a valid NP
    /// - The Scan methods call ScanComplexNP(), which will fill in the noun directly if successful.
    /// - So this is only called after scanning for NPs with no modifiers.
    /// </summary>
    /// <returns></returns>
    protected override Noun GetConcept()
    {
        var text = Text;

        if (Number == Syntax.Number.Plural || beginsWithDeterminer || ForceCommonNoun)
            return GetCommonNoun(text);

        return GetProperNoun(text);
    }

    private Noun GetProperNoun(string[] text)
    {
        return Noun.Find(text) ?? new ProperNoun(text);
    }

    private Noun GetCommonNoun(string[] text)
    {
        var noun = (CommonNoun)Noun.Find(text);
        if (noun != null)
        {
            var singular = noun.SingularForm.SameAs(text);
            if (singular && Number == Syntax.Number.Plural && !noun.SingularForm.SameAs(noun.PluralForm))
                throw new GrammaticalError("Singular noun used without 'a' or 'an'", Text);
            if (!singular && Number == Syntax.Number.Singular)
                throw new GrammaticalError("Plural noun used with 'a' or 'an'", Text);
            return noun;
        }

        noun = new CommonNoun();

        if (Number == Syntax.Number.Singular)
            noun.SingularForm = text;
        else
            // Note: this guarantees there is a singular form.
            noun.PluralForm = text;

        MaybeLoadDefinitions(noun);

        return noun;
    }

    /// <summary>
    /// The grammatical Number of this NP (singular, plural, or null if unmarked or not yet known)
    /// </summary>
    public Syntax.Number? Number { get; set; }

    /// <summary>
    /// The explicitly specified count of the NP, if any.
    /// For example, "ten cats"
    /// </summary>
    public int? ExplicitCount
    {
        get => _explicitCount;
        set
        {
            _explicitCount = value;
            if (value != null)
                Number = _explicitCount == 1 ? Syntax.Number.Singular : Syntax.Number.Plural;
        }
    }
    // ReSharper disable once InconsistentNaming
    private int? _explicitCount;

    private string DebugText => Text.Untokenize();

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        Modifiers.Clear();
        Number = null;
        ExplicitCount = null;
        beginsWithDeterminer = false;
        ForceCommonNoun = false;
    }
}