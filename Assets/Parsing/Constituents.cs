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

using System;
using System.Collections.Generic;

/// <summary>
/// Rules for parsing the top-level syntax of sentences.
/// </summary>
public partial class Parser
{

    #region Constituent information

    private void InitializeConstituents()
    {
        Subject = new NP(this) {Name = "Subject"};
        Object = new NP(this) {Name = "Object"};

        Verb = new VerbSegment(this) {Name = "Verb"};
        Verb2 = new VerbSegment(this) {Name = "Verb2"};

        SubjectNounList = new ReferringExpressionList<NP, Noun>(this, () => new NP(this))
            {SanityCheck = SentencePattern.ForceBaseForm, Name = "Subjects"};
        PredicateAPList =
            new ReferringExpressionList<AP, Adjective>(this, () => new AP(this))
                {Name = "Adjectives"};
        PredicateAP = new AP(this) {Name = "Adjective"};
        ListName = new Segment(this) {Name = "ListName"};
        ButtonName = new Segment(this) {Name = "ButtonName"};
        Text = new Segment(this) {Name = "AnyText", AllowListConjunctions = true};
        Quantifier = new QuantifyingDeterminer(this) {Name = "one/many/other"};

    OptionalAll = new SimpleClosedClassSegment(this,
            "all", "any", "every")
        { Name = "[all]", Optional = true };

    OptionalAlways = new SimpleClosedClassSegment(this,
            "always")
        { Name = "[always]", Optional = true };

    ExistNotExist = new SimpleClosedClassSegment(this,
            "exist", new[] {"not", "exist"}, new[] { "never", "exist" })
        {Name = "exist/not exist"};

    RareCommon =
        new ClosedClassSegmentWithValue<float>(this,
                new KeyValuePair<object, float>(new[] {"very", "rare"}, 0.05f),
                new KeyValuePair<object, float>("rare", 0.15f),
                new KeyValuePair<object, float>("common", 0.85f),
                new KeyValuePair<object, float>(new[] {"very", "common"}, 0.95f))
            {Name = "rare/common"};

    CanMust = new SimpleClosedClassSegment(this,
            "can", "must")
        {Name = "can/must"};

    CanNot = new SimpleClosedClassSegment(this,
            "cannot", "never", new[] {"can", "not"}, new[] {"can", "'", "t"},
            new[] {"do", "not"}, new[] {"do", "'", "t"})
        {Name = "cannot"};

    Reflexive = new SimpleClosedClassSegment(this,
            "itself", "himself", "herself", "themselves")
        {Name = "itself"};

    Always = new SimpleClosedClassSegment(this,
            "must", "always")
        {Name = "always"};

    EachOther = new SimpleClosedClassSegment(this,
            new[] {"each", "other"}, new[] {"one", "another"})
        {Name = "each other"};
    }


    /// <summary>
    /// Reinitialize global variables that track the values of constituents.
    /// Called each time a new syntax rule is tried.
    /// </summary>
    private void ResetConstituentInformation()
    {
        Subject.Reset();
        Verb.Reset();
        Verb2.Reset();
        Object.Reset();
        PredicateAP.Reset();
        SubjectNounList.Reset();
        PredicateAPList.Reset();
        Quantifier.Reset();
        VerbNumber = null;
    }

            /// <summary>
        /// Segment for the subject of a sentence
        /// </summary>
        public NP Subject;

        /// <summary>
        /// Segment for the object of a sentence
        /// </summary>
        public NP Object;

        public VerbSegment Verb;
        public VerbSegment Verb2;

        /// <summary>
        /// Used when the subject of a sentences is a list of NPs
        /// </summary>
        public ReferringExpressionList<NP, Noun> SubjectNounList;

        /// <summary>
        /// Used when the predicate of a sentences is a list of APs
        /// </summary>
        public ReferringExpressionList<AP, Adjective> PredicateAPList;

        /// <summary>
        /// Segment for the AP forming the predicate of a sentences
        /// </summary>
        public AP PredicateAP;

        /// <summary>
        /// Segment for the file name of a list of values (e.g. for possible names of characters)
        /// </summary>
        public Segment ListName;

        /// <summary>
        /// Segment for the name of a button being created
        /// </summary>
        public Segment ButtonName;
        
        /// <summary>
        /// Free-form text, e.g. from a quotation.
        /// </summary>
        public Segment Text;

        public QuantifyingDeterminer Quantifier;

        /// <summary>
        /// The lower bound of a range appearing in the definition of a numeric property
        /// </summary>
        public float ParsedLowerBound;

        /// <summary>
        /// The upper bound of a range appearing in the definition of a numeric property
        /// </summary>
        public float ParsedUpperBound;
        
        public Number? VerbNumber;
    #endregion

    /// <summary>
    /// Recognizes conjugations of the verb to be.
    /// </summary>
    public  readonly Func<bool> Is;

    /// <summary>
    /// Recognizes conjugations of the verb to have
    /// </summary>
    public readonly Func<bool> Has;

    public SimpleClosedClassSegment OptionalAll;

    public SimpleClosedClassSegment OptionalAlways;

    public SimpleClosedClassSegment ExistNotExist;

    public ClosedClassSegmentWithValue<float> RareCommon;

    public SimpleClosedClassSegment CanMust;

    public SimpleClosedClassSegment CanNot;

    public SimpleClosedClassSegment Reflexive;

    public SimpleClosedClassSegment Always;

    public SimpleClosedClassSegment EachOther;

    /// <summary>
    /// Recognizes numbers and stores them in lowerBound
    /// </summary>
    public Func<bool> LowerBound;

    /// <summary>
    /// Recognizes numbers and stores them in upperBound
    /// </summary>
    public Func<bool> UpperBound;

    
    #region Feature checks for syntax rules
    private bool SubjectVerbAgree()
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

    private bool VerbBaseForm()
    {
        if (Verb.Text[0] != "be" && VerbNumber == Number.Singular)
            return false;
        VerbNumber = Number.Plural;
        Verb.Conjugation = VerbConjugation.BaseForm;
        return true;
    }

    private bool VerbGerundForm() => VerbGerundForm(Verb);
    private bool Verb2GerundForm() => VerbGerundForm(Verb2);

    private bool VerbGerundForm(VerbSegment s)
    {
        s.Conjugation = VerbConjugation.Gerund;
        return Inflection.IsGerund(s.Text);
    }

    private bool SubjectDefaultPlural()
    {
        if (Subject.Number == null)
            Subject.Number = Number.Plural;
        return true;
    }

    private bool SubjectPlural() => Subject.Number == Number.Plural;

    //private static bool ObjectNonSingular() => Object.Number == null || Object.Number == Number.Plural;

    /// <summary>
    /// Object is not marked plural.  If number is ambiguous, force it to singular.
    /// </summary>
    /// <returns></returns>
    private bool ObjectSingular()
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
    private bool ObjectExplicitlySingular() => Object.Number == Number.Singular && Object.BeginsWithDeterminer;

    /// <summary>
    /// Subject is syntactically singular, i.e. it starts with "a", "an", etc.
    /// </summary>
    private bool SubjectExplicitlySingular() => Subject.Number == Number.Singular && Subject.BeginsWithDeterminer;

    private bool ObjectQuantifierAgree()
    {
        Object.Number = Quantifier.IsPlural ? Number.Plural : Number.Singular;
        return true;
    }

    /// <summary>
    /// Used for sentential forms that can't accept adjectives in their subject.
    /// </summary>
    /// <returns></returns>
    private bool SubjectUnmodified()
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
    private bool ObjectUnmodified()
    {
        if (Object.Modifiers.Count > 0)
            throw new GrammaticalError($"The noun '{Object.Text.Untokenize()}' cannot take adjectives", 
                $"The noun '<i>{Object.Text.Untokenize()}</i>' cannot take any adjectives or other modifiers as the object of this sentence pattern.");
        return true;
    }

    private bool ObjectCommonNoun()
    {
        Object.ForceCommonNoun = true;
        return true;
    }

    
    private bool SubjectProperNoun()
    {
        return Subject.Noun is ProperNoun;
    }
    private bool SubjectCommonNoun()
    {
        Subject.ForceCommonNoun = true;
        return true;
    }

    public static bool ListConjunction(string currentToken) => currentToken == "and" || currentToken == "or";
    #endregion
}