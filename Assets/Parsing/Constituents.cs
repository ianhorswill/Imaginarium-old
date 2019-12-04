#region Copyright
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

using static Parser;
using System;
using System.Collections.Generic;

/// <summary>
/// Rules for parsing the top-level syntax of sentences.
/// </summary>
public partial class Syntax
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
    private static readonly Func<bool> Is = MatchCopula;

    /// <summary>
    /// Recognizes conjugations of the verb to have
    /// </summary>
    private static readonly Func<bool> Has = MatchHave;

    private static readonly ClosedClassSegmentWithValue<float> RareCommon =
        new ClosedClassSegmentWithValue<float>(
                new KeyValuePair<object, float>(new[] {"very", "rare"}, 0.05f),
                new KeyValuePair<object, float>("rare", 0.15f),
                new KeyValuePair<object, float>("common", 0.85f),
                new KeyValuePair<object, float>(new[] {"very", "common"}, 0.95f))
            {Name = "rare/common"};

    private static readonly ClosedClassSegment CanMust = new ClosedClassSegment(
            "can", "must")
        {Name = "can/must"};

    private static readonly ClosedClassSegment CanNot = new ClosedClassSegment(
            "cannot", "never", new[] {"can", "not"}, new[] {"can", "'", "t"},
            new[] {"do", "not"}, new[] {"do", "'", "t"})
        {Name = "cannot"};

    private static readonly ClosedClassSegment Reflexive = new ClosedClassSegment(
            "itself", "himself", "herself", "themselves")
        {Name = "itself"};

    private static readonly ClosedClassSegment Always = new ClosedClassSegment(
            "must", "always")
        {Name = "always"};

    private static readonly ClosedClassSegment EachOther = new ClosedClassSegment(
            new[] {"each", "other"}, new[] {"one", "another"})
        {Name = "each other"};

    /// <summary>
    /// Recognizes numbers and stores them in lowerBound
    /// </summary>
    private static readonly Func<bool> LowerBound = () => MatchNumber(out Parser.Current.LowerBound);

    /// <summary>
    /// Recognizes numbers and stores them in upperBound
    /// </summary>
    private static readonly Func<bool> UpperBound = () => MatchNumber(out Parser.Current.UpperBound);

    #endregion
}