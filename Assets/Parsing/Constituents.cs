﻿#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Constituents.cs" company="Ian Horswill">
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

/// <summary>
/// Rules for parsing the top-level syntax of sentences.
/// </summary>
public static partial  class Parser
{
    #region Constituent information

    /// <summary>
    /// Segment for the subject of a sentence
    /// </summary>
    public static NP Subject => Current.Subject;

    /// <summary>
    /// Segment for the object of a sentence
    /// </summary>
    public static NP Object => Current.Object;

    public static VerbSegment Verb => Current.Verb;
    public static VerbSegment Verb2 => Current.Verb2;

    /// <summary>
    /// Used when the subject of a sentences is a list of NPs
    /// </summary>
    public static ReferringExpressionList<NP, Noun> SubjectNounList => Current.SubjectNounList;

    /// <summary>
    /// Used when the predicate of a sentences is a list of APs
    /// </summary>
    public static ReferringExpressionList<AP, Adjective> PredicateAPList => Current.PredicateAPList;

    /// <summary>
    /// Segment for the AP forming the predicate of a sentences
    /// </summary>
    public static AP PredicateAP => Current.PredicateAP;

    /// <summary>
    /// Segment for the file name of a list of values (e.g. for possible names of characters)
    /// </summary>
    public static Segment ListName => Current.ListName;

    /// <summary>
    /// Segment for the name of a button to be created
    /// </summary>
    public static Segment ButtonName => Current.ButtonName;

    /// <summary>
    /// Free-form text, e.g. from a quotation.
    /// </summary>
    public static Segment Text => Current.Text;

    public static QuantifyingDeterminer Quantifier => Current.Quantifier;

    /// <summary>
    /// The lower bound of a range appearing in the definition of a numeric property
    /// </summary>
    // ReSharper disable once InconsistentNaming
    private static float lowerBound => Current.LowerBound;

    /// <summary>
    /// The upper bound of a range appearing in the definition of a numeric property
    /// </summary>
    // ReSharper disable once InconsistentNaming
    private static float upperBound => Current.UpperBound;

    /// <summary>
    /// The number feature (singular, plural) of the verb of a sentence, or null if unknown
    /// </summary>
    public static Number? VerbNumber
    {
        get => Current.VerbNumber;
        set => Current.VerbNumber = value;
    }

    /// <summary>
    /// Recognizes conjugations of the verb to be.
    /// </summary>
    public static readonly Func<bool> Is = MatchCopula;

    /// <summary>
    /// Recognizes conjugations of the verb to have
    /// </summary>
    public static readonly Func<bool> Has = MatchHave;

    private static readonly SimpleClosedClassSegment OptionalAll = new SimpleClosedClassSegment(
        "all", "any", "every")
        { Name = "[all]", Optional = true };

    private static readonly SimpleClosedClassSegment OptionalAlways = new SimpleClosedClassSegment(
            "always")
        { Name = "[always]", Optional = true };

    private static readonly SimpleClosedClassSegment ExistNotExist = new SimpleClosedClassSegment(
            "exist", new[] {"not", "exist"}, new[] { "never", "exist" })
        {Name = "exist/not exist"};

    private static readonly ClosedClassSegmentWithValue<float> RareCommon =
        new ClosedClassSegmentWithValue<float>(
                new KeyValuePair<object, float>(new[] {"very", "rare"}, 0.05f),
                new KeyValuePair<object, float>("rare", 0.15f),
                new KeyValuePair<object, float>("common", 0.85f),
                new KeyValuePair<object, float>(new[] {"very", "common"}, 0.95f))
            {Name = "rare/common"};

    private static readonly SimpleClosedClassSegment CanMust = new SimpleClosedClassSegment(
            "can", "must")
        {Name = "can/must"};

    private static readonly SimpleClosedClassSegment CanNot = new SimpleClosedClassSegment(
            "cannot", "never", new[] {"can", "not"}, new[] {"can", "'", "t"},
            new[] {"do", "not"}, new[] {"do", "'", "t"})
        {Name = "cannot"};

    private static readonly SimpleClosedClassSegment Reflexive = new SimpleClosedClassSegment(
            "itself", "himself", "herself", "themselves")
        {Name = "itself"};

    private static readonly SimpleClosedClassSegment Always = new SimpleClosedClassSegment(
            "must", "always")
        {Name = "always"};

    private static readonly SimpleClosedClassSegment EachOther = new SimpleClosedClassSegment(
            new[] {"each", "other"}, new[] {"one", "another"})
        {Name = "each other"};

    /// <summary>
    /// Recognizes numbers and stores them in lowerBound
    /// </summary>
    public static readonly Func<bool> LowerBound = () => MatchNumber(out Current.LowerBound);

    /// <summary>
    /// Recognizes numbers and stores them in upperBound
    /// </summary>
    public static readonly Func<bool> UpperBound = () => MatchNumber(out Current.UpperBound);

    
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
    private static bool ObjectExplicitlySingular() => Object.Number == Number.Singular && Object.BeginsWithDeterminer;

    /// <summary>
    /// Subject is syntactically singular, i.e. it starts with "a", "an", etc.
    /// </summary>
    private static bool SubjectExplicitlySingular() => Subject.Number == Number.Singular && Subject.BeginsWithDeterminer;

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
            throw new GrammaticalError($"The noun '{Subject.Text.Untokenize()}' cannot take adjectives in this context",
                $"The noun '{Subject.Text.Untokenize()}' cannot take adjectives as the subject of this sentence pattern");
        return true;
    }

    /// <summary>
    /// Used for sentential forms that can't accept adjectives in their object.
    /// </summary>
    /// <returns></returns>
    private static bool ObjectUnmodified()
    {
        if (Object.Modifiers.Count > 0)
            throw new GrammaticalError($"The noun '{Object.Text.Untokenize()}' cannot take adjectives", 
                $"The noun '<i>{Object.Text.Untokenize()}</i>' cannot take any adjectives or other modifiers as the object of this sentence pattern.");
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

    #endregion
}