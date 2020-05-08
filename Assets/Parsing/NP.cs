#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NP.cs" company="Ian Horswill">
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
                throw new GrammaticalError($"{Text.Untokenize()} is not a common noun",
                    $"I was expecting the term '<i>{Text.Untokenize()}</i>' to be a common noun (a kind of person, place, thing, etc.), but it isn't");
            return n;
        }
    }

    /// <summary>
    /// The modifiers (adjectives or other common nouns) applied to the CommonNoun head, if any.
    /// For example, in "quick, brown fox", fox is the CommonNoun and quick and brown are modifiers.
    /// </summary>
    public List<MonadicConceptLiteral> Modifiers = new List<MonadicConceptLiteral>();

    /// <summary>
    /// True if the segment starts with a determiner
    /// </summary>
    public bool BeginsWithDeterminer;
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
            if (!EndOfInput && CurrentToken == token)
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
            if (!EndOfInput && endPredicate(CurrentToken))
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
        if (EndOfInput)
            return;

        BeginsWithDeterminer = true;
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
            BeginsWithDeterminer = false;
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
        MonadicConcept nextConcept;
        MonadicConceptLiteral last = null;
        Modifiers.Clear();
        do
        {
            var isPositive = true;
            if (EndOfInput)
                break;
            var tok = CurrentToken.ToLower();
            if (tok == "not" || tok == "non")
            {
                isPositive = false;
                SkipToken();
                if (EndOfInput)
                    return false;
                if (CurrentToken == "-")
                    SkipToken();
            }
            nextConcept = MatchTrie(MonadicConcept.Trie);
            if (nextConcept != null)
            {
                var next = new MonadicConceptLiteral(nextConcept, isPositive);

                if (last != null)
                    Modifiers.Add(last);
                last = next;
                if (!EndOfInput && CurrentToken == ",")
                    SkipToken();
            }
        } while (nextConcept != null);

        if (last != null && last.Concept is Noun n)
        {
            CachedConcept = n;
            if (!Number.HasValue)
                // Only update if Number wasn't already set by a determiner.
                // This is to get around nouns that are their own plurals.
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

        if (Number == Syntax.Number.Plural || BeginsWithDeterminer || ForceCommonNoun)
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
                throw new GrammaticalError($"The singular noun '{Text.Untokenize()}' was used without 'a' or 'an' before it", 
                    $"The singular noun '<i>{Text.Untokenize()}</i>' was used without 'a' or 'an' before it");
            if (!singular && Number == Syntax.Number.Singular)
                throw new GrammaticalError($"The plural noun '{Text.Untokenize()}' was used with 'a' or 'an'",
                    $"The plural noun '<i>{Text.Untokenize()}</i>' was used with 'a' or 'an' before it");
            return noun;
        }

        noun = new CommonNoun();

        if (!Number.HasValue)
            // Don't know syntactically if it's supposed to be singular or plural, so guess.
            Number = Inflection.NounAppearsPlural(text)
                ? Syntax.Number.Plural
                : Syntax.Number.Singular;
        if (Number == Syntax.Number.Singular)
            noun.SingularForm = text;
        else
            // Note: this guarantees there is a singular form.
            noun.PluralForm = text;

        Driver.AppendResponseLine($"Learned the new common noun <b><i>{noun.SingularForm.Untokenize()}</i></b>.");

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
        BeginsWithDeterminer = false;
        ForceCommonNoun = false;
    }
}