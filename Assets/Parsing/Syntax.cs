#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Syntax.cs" company="Ian Horswill">
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

using static Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

/// <summary>
/// Rules for parsing the top-level syntax of sentences.
/// </summary>
[DebuggerDisplay("{" + nameof(HelpDescription) + "}")]
public partial class Syntax
{
    /// <summary>
    /// Used in SubjectNounList to ensure all NPs are in base form (singular but no determiner)
    /// </summary>
    public static bool ForceBaseForm(NP np)
    {
        np.ForceCommonNoun = true;
        if (np.Number == Number.Plural)
            return false;
        np.Number = Number.Singular;
        return true;
    }

    #region Feature checks for syntax rules
    private static bool SubjectVerbAgree()
    {
        if (Subject.Number == null)
        {
            Subject.Number = VerbNumber;
            return true;
        }

        if (VerbNumber == null)
            VerbNumber = Subject.Number;
        return VerbNumber == null || VerbNumber == Subject.Number;
    }

    private static bool VerbBaseForm()
    {
        if (Verb.Text[0] != "be" && VerbNumber == Number.Singular)
            return false;
        VerbNumber = Number.Plural;
        Verb.Conjugation = VerbConjugation.BaseForm;
        return true;
    }

    private static bool VerbGerundForm() => VerbGerundForm(Verb);
    private static bool Verb2GerundForm() => VerbGerundForm(Verb2);

    private static bool VerbGerundForm(VerbSegment s)
    {
        s.Conjugation = VerbConjugation.Gerund;
        return Inflection.IsGerund(s.Text);
    }

    private static bool SubjectDefaultPlural()
    {
        if (Subject.Number == null)
            Subject.Number = Number.Plural;
        return true;
    }

    private static bool SubjectPlural() => Subject.Number == Number.Plural;

    //private static bool ObjectNonSingular() => Object.Number == null || Object.Number == Number.Plural;

    /// <summary>
    /// Object is not marked plural.  If number is ambiguous, force it to singular.
    /// </summary>
    /// <returns></returns>
    private static bool ObjectSingular()
    {
        if (Object.Number == Number.Plural)
            throw new GrammaticalError($"The noun '{Object.Text}' should be in singular form in this context",
                $"The noun '<i>{Object.Text}<i>' should be in singular form in this context");

        Object.Number = Number.Singular;
        return true;
    }

    /// <summary>
    /// Object is syntactically singular, i.e. it starts with "a", "an", etc.
    /// </summary>
    private static bool ObjectExplicitlySingular() => Object.Number == Number.Singular;

    private static bool ObjectQuantifierAgree()
    {
        Object.Number = Quantifier.IsPlural ? Number.Plural : Number.Singular;
        return true;
    }

    /// <summary>
    /// Used for sentential forms that can't accept adjectives in their subject.
    /// </summary>
    /// <returns></returns>
    private static bool SubjectUnmodified()
    {
        if (Subject.Modifiers.Count > 0)
            throw new GrammaticalError($"The noun '{Subject.Text}' cannot take adjectives in this context",
                $"The noun '{Subject.Text}' cannot take adjectives as the subject of this sentence pattern");
        return true;
    }

    /// <summary>
    /// Used for sentential forms that can't accept adjectives in their object.
    /// </summary>
    /// <returns></returns>
    private static bool ObjectUnmodified()
    {
        if (Object.Modifiers.Count > 0)
            throw new GrammaticalError($"The noun '{Object.Text}' cannot take adjectives", 
                $"The noun '<i>{Object.Text}</i>' cannot take any adjectives or other modifiers as the object of this sentence pattern.");
        return true;
    }

    private static bool ObjectCommonNoun()
    {
        Object.ForceCommonNoun = true;
        return true;
    }

    
    private static bool SubjectProperNoun()
    {
        return Subject.Noun is ProperNoun;
    }
    private static bool SubjectCommonNoun()
    {
        Subject.ForceCommonNoun = true;
        return true;
    }

    public static bool ListConjunction(string currentToken) => currentToken == "and" || currentToken == "or";
    #endregion

    #region Constructors
    // ReSharper disable once CoVariantArrayConversion
    public Syntax(params string[] tokens) : this(() => tokens) { }

    public Syntax(Func<object[]> makeConstituents)
    {
        this.makeConstituents = makeConstituents;
    }

    /// <summary>
    /// Adds an action to a Syntax rule.
    /// This is here only so that the syntax constructor can take the constituents as a params arg,
    /// which makes the code a little more readable.
    /// </summary>
    public Syntax Action(Action a)
    {
        action = a;
        return this;
    }

    /// <summary>
    /// Adds a set of feature checks to a Syntax rule.
    /// This is here only so that the syntax constructor can take the constituents as a params arg,
    /// which makes the code a little more readable.
    /// </summary>
    public Syntax Check(params Func<bool>[] checks)
    {
        validityTests = checks;
        return this;
    }
    #endregion

    /// <summary>
    /// Closed class words used in this sentence template
    /// </summary>
    public IEnumerable<string> Keywords
    {
        get
        {
            foreach (var c in makeConstituents())
                switch (c)
                {
                    case string s:
                        yield return s;
                        break;

                    case ClosedClassSegment ccs:
                        foreach (var s in ccs.Keywords)
                            yield return s;
                        break;
                }
        }
    }

    /// <summary>
    /// True if the tokens have a word in common with the keywords of this rule
    /// </summary>
    /// <param name="tokens">Words to check against the keywords of this rule.</param>
    /// <returns>True if there is a word in common</returns>
    public bool HasCommonKeywords(IEnumerable<string> tokens) => tokens.Any(t => Keywords.Contains(t));

    /// <summary>
    /// Return all rules whose keywords overlap the specified set of tokens
    /// </summary>
    /// <param name="tokens">Words to check against rule keywords</param>
    /// <returns>Rules with keywords in common</returns>
    public static IEnumerable<Syntax> RulesMatchingKeywords(IEnumerable<string> tokens) =>
        AllRules.Where(r => r.HasCommonKeywords(tokens));

    /// <summary>
    /// Try to make a syntax rule and run its action if successful.
    /// </summary>
    /// <returns>True on success</returns>
    public bool Try()
    {
        ResetParser();
        var old = State;

        if (MatchConstituents())
            if (EndOfInput)
            {
                // Check validity tests and fail if one fails
                if (validityTests != null)
                {
                    foreach (var test in validityTests)
                        if (!test())
                        {
                            if (LogMatch)
                            {
                                var d = (Delegate)test;
                                Driver.AppendResponseLine("Validity test failed: "+d.Method.Name);
                            }

                            goto fail;
                        }
                }

                action();
                return true;
            }
            else if (LogMatch)
            {
                Driver.AppendResponseLine("Remaining input blocked match: "+Parser.CurrentToken);
            }

        fail:
        ResetTo(old);
        return false;
    }

    /// <summary>
    /// Try to match the constituents of a syntax rule, resetting the parser on failure.
    /// </summary>
    /// <returns>True if successful</returns>
    private bool MatchConstituents()
    {
        var constituents = makeConstituents();

        if (constituents[0] is string firstToken 
            && string.Compare(CurrentToken, firstToken, StringComparison.InvariantCultureIgnoreCase) != 0)
            // Fast path.  This also reduces spam in the logging output
            return false;

        if (LogMatch) Driver.AppendResponseLine("Try parse rule: " + SentencePatternDescription);

        for (int i = 0; i < constituents.Length; i++)
        {
            var c = constituents[i];
            if (LogMatch)
            {
                var conName = ConstituentToString(c);
                Driver.AppendResponseLine($"Constituent {conName}");
                Driver.AppendResponseLine($"Remaining input: {Parser.RemainingInput}");
            }
            if (BreakOnMatch)
                Debugger.Break();
            if (c is string str)
            {
                if (!Match(str))
                    return false;
            }
            else if (c is Segment seg)
            {
                if (i == constituents.Length - 1)
                {
                    // Last one
                    if (!seg.ScanToEnd())
                        return false;
                }
                else
                {
                    var next = constituents[i + 1];
                    if (next is string nextStr)
                    {
                        if (!seg.ScanTo(nextStr))
                            return false;
                    }
                    else if (ReferenceEquals(next, Is))
                    {
                        if (!seg.ScanTo(IsCopula))
                            return false;
                    }
                    else if (ReferenceEquals(next, Has))
                    {
                        if (!seg.ScanTo(IsHave))
                            return false;
                    }
                    else if (next is SimpleClosedClassSegment s)
                    {
                        if (!seg.ScanTo(s.IsPossibleStart))
                            return false;
                    }
                    else if (seg is SimpleClosedClassSegment)
                    {
                        if (!seg.ScanTo(tok => true))
                            return false;
                    }
                    else if (next is QuantifyingDeterminer q)
                    {
                        if (!seg.ScanTo(q.IsQuantifier))
                            return false;
                    }
                    else if (seg is QuantifyingDeterminer)
                    {
                        if (!seg.ScanTo(tok => true))
                            return false;
                    }
                    else throw new ArgumentException("Don't know how to scan to the next constituent type");
                }

                if (LogMatch)
                {
                    var text = seg.Text;
                    var asString = text != null ? text.Untokenize() : "(null)";
                    Driver.AppendResponseLine($"{seg.Name} matches {asString}");
                }
            }
            else if (c is Func<bool> test)
            {
                if (!test())
                    return false;
            }
            else throw new ArgumentException($"Unknown type of constituent {c}");

        }

        if (LogMatch) Driver.AppendResponseLine("Succeeded parsing rule: " + SentencePatternDescription);
        return true;
    }

    private static object ConstituentToString(object c)
    {
        var conName = c is Segment seg ? seg.Name : c;
        return conName;
    }

    /// <summary>
    /// Matching routines for the constituents of the sentential form, in order.
    /// For example: Subject, Is, Object
    /// </summary>
    private readonly Func<object[]> makeConstituents;
    /// <summary>
    /// Procedure to run if this sentential form matches the input.
    /// This procedure should update the ontology based on the data stored in the constituents
    /// during the matching phase.
    /// </summary>
    private Action action;
    /// <summary>
    /// Additional sanity checks to perform, e.g. for checking plurality.
    /// </summary>
    private Func<bool>[] validityTests;

    public bool IsCommand;
    public bool BreakOnMatch;
    /// <summary>
    /// True if logging all parsing
    /// </summary>
    public static bool LogAllParsing;
    /// <summary>
    /// True if logging this one rule, regardless of LogAllParsing
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public bool _logMatch;

    /// <summary>
    /// True if we should log the parsing of this rule right now.
    /// </summary>
    public bool LogMatch => _logMatch || LogAllParsing;

    public Syntax DebugMatch()
    {
        BreakOnMatch = true;
        return this;
    }

    public Syntax Log()
    {
        _logMatch = true;
        return this;
    }

    public Syntax Command()
    {
        IsCommand = true;
        return this;
    }

    /// <summary>
    /// User-facing description of this form.
    /// </summary>
    public string DocString;

    /// <summary>
    /// Adds the specified documentation string to the Syntax form.
    /// </summary>
    public Syntax Documentation(string doc)
    {
        DocString = doc;
        return this;
    }

    private static readonly StringBuilder Buffer = new StringBuilder();
    public string HelpDescription
    {
        get
        {
            Buffer.Length = 0;
            var firstOne = true;
            Buffer.Append("<b>");
            foreach (var c in makeConstituents())
            {
                if (firstOne)
                    firstOne = false;
                else Buffer.Append(' ');

                Buffer.Append(ConstituentName(c));
            }

            Buffer.Append("</b>\n");
            Buffer.Append(DocString??"");
            return Buffer.ToString();
        }
    }

    public string SentencePatternDescription
    {
        get
        {
            Buffer.Length = 0;
            var firstOne = true;
            Buffer.Append("<b>");
            foreach (var c in makeConstituents())
            {
                if (firstOne)
                    firstOne = false;
                else Buffer.Append(' ');

                Buffer.Append(ConstituentName(c));
            }

            Buffer.Append("</b>");
            return Buffer.ToString();
        }
    }

    private static string ConstituentName(object c)
    {
        switch (c)
        {
            case string s:
                return s;

            case ClosedClassSegment ccs:
                return ccs.Name;

            case Segment seg:
                return $"<i><color=grey>{seg.Name}</color></i>";

            case Func<bool> f:
                if (f == Is)
                    return "is/are";
                if (f == Has)
                    return "have/has";
                if (f == LowerBound)
                    return "<i><color=grey>LowerBound</color></i>";
                if (f == UpperBound)
                    return "<i><color=grey>UpperBound</color></i>";
                return $"<i>{f}</i>";

            default:
                return $"<i>{c}</i>";
        }
    }

    public static bool SingularDeterminer(string word) => word == "a" || word == "an";

    /// <summary>
    /// Grammatical number feature
    /// </summary>
    public enum Number
    {
        Singular,
        Plural
    }
}