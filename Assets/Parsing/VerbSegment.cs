#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VerbSegment.cs" company="Ian Horswill">
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
using System.Diagnostics;
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

    public VerbConjugation Conjugation;

    public override void Reset()
    {
        base.Reset();
        Conjugation = VerbConjugation.BaseForm;
    }

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
        } else if (base.ScanToEnd(failOnConjunction))
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

        switch (Conjugation)
        {
            case VerbConjugation.BaseForm:
                verb.BaseForm = text;
                break;

            case VerbConjugation.Gerund:
                verb.GerundForm = text;
                break;

            case VerbConjugation.ThirdPerson:
                if (Syntax.VerbNumber == Syntax.Number.Singular)
                    verb.SingularForm = text;
                else
                    // Note: this guarantees there is a singular form.
                    verb.PluralForm = text;
                break;
        }

        Driver.AppendResponseLine($"Learned new verb <b><i>{verb.StandardName.Untokenize()}</i></b>.");

        MaybeLoadDefinitions(verb);

        return verb;
    }
}