#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Syntax.cs" company="Ian Horswill">
// Copyright (C) 2018, 2019 Ian Horswill
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
using System.IO;
using System.Linq;
using System.Text;
using CatSAT.NonBoolean.SMT.Float;
using CatSAT.NonBoolean.SMT.MenuVariables;

/// <summary>
/// Rules for parsing the top-level syntax of sentences.
/// </summary>
public class Syntax
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
            new KeyValuePair<object, float>(new[] {"very", "common"}, 0.95f) )
        {Name = "rare/common"};

    private static readonly ClosedClassSegment CanMust = new ClosedClassSegment(
            "can", "must" )
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
    #endregion

    /// <summary>
    /// Rules for the different sentential forms understood by the system.
    /// Each consists of a pattern to recognize the form and store its components in static fields such
    /// as Subject, Object, and VerbNumber, and an Action to perform updates to the ontology based on
    /// the data stored in those static fields.  The Check option is used to insure features are properly
    /// set.
    /// </summary>
    public static readonly Syntax[] AllRules =
    {
        new Syntax("help")
            .Action(() =>
            {
                var b = new StringBuilder();
                b.Append("<size=14><color=white>");
                foreach (var r in AllRules) b.Append(r.HelpDescription);
                b.Append("</color></size>");

                Driver.CommandResponse = b.ToString();
            })
            .Documentation("Prints this list of commands"),

        new Syntax(() => new object[] { "imagine", Object })
            .Action(() =>
            {
                var countRequest = Object.ExplicitCount;
                var count = countRequest ?? (Object.Number == Number.Plural?9:1);
                Generator.Current = new Generator(Object.CommonNoun, Object.Modifiers, count);
            })
            .Command()
            .Documentation("Generates one or more Objects.  For example, 'imagine a cat' or 'imagine long-haired cats'."),

        new Syntax("undo")
            .Action( () => History.Undo() )
            .Command()
            .Documentation("Undoes the last change to the ontology."), 

        new Syntax("start", "over")
            .Action( History.Clear )
            .Command()
            .Documentation("Tells the system to forget everything you've told it about the world."), 
        
        new Syntax(() => new object[] { "save", ListName })
            .Action(() =>
            {
                History.Save(Parser.DefinitionFilePath(ListName.Text.Untokenize()));
            })
            .Command()
            .Documentation("Saves assertions to a file."),

        new Syntax(() => new object[] { "edit", ListName })
            .Action(() =>
            {
                var definitionFilePath = Parser.DefinitionFilePath(ListName.Text.Untokenize());

                Ontology.EraseConcepts();
                Parser.LoadDefinitions(definitionFilePath);
                History.Edit(definitionFilePath);
            })
            .Command()
            .Documentation("Add new assertions to a file.  Use save command to save changes."),
        
        new Syntax(() => new object[] { Subject, CanMust, Verb, Quantifier, Object })
            .Action(() =>
            {
                var verb = Verb.Verb;
                verb.SubjectKind = Subject.CommonNoun;
                verb.ObjectKind = Object.CommonNoun;
                verb.IsFunction = !Quantifier.IsPlural;
                verb.IsAntiReflexive = Quantifier.IsOther;
                verb.IsTotal = CanMust.Match[0] == "must";
            })
            .Check(VerbBaseForm, ObjectUnmodified, ObjectQuantifierAgree, SubjectCommonNoun, ObjectCommonNoun),

        new Syntax(() => new object[] { Verb, "is", RareCommon })
            .Action(() => Verb.Verb.Density = RareCommon.Value), 

        new Syntax(() => new object[] { Verb, "and", Verb2, "are", "mutually", "exclusive" })
            .Action(() =>
            {
                Verb.Verb.MutualExclusions.Add(Verb2.Verb);
            }), 

        new Syntax(() => new object[] { Verb, "implies", Verb2 })
            .Action(() =>
            {
                Verb.Verb.Generalizations.Add(Verb2.Verb);
            }), 

        new Syntax(() => new object[] { Subject, "are", RareCommon })
            .Action(() => Subject.CommonNoun.InitialProbability = RareCommon.Value)
            .Check(SubjectPlural, SubjectUnmodified), 

        new Syntax(() => new object[] { Subject, CanNot, Verb, Reflexive })
            .Action(() =>
            {
                var verb = Verb.Verb;
                verb.SubjectKind = verb.ObjectKind = Subject.CommonNoun;
                verb.IsAntiReflexive = true;
            })
            .Check(VerbBaseForm, SubjectCommonNoun),

        new Syntax(() => new object[] { Subject, Always, Verb, Reflexive })
            .Action(() =>
            {
                var verb = Verb.Verb;
                verb.SubjectKind = verb.ObjectKind = Subject.CommonNoun;
                verb.IsReflexive = true;
            })
            .Check(VerbBaseForm, SubjectCommonNoun),

        new Syntax(() => new object[] { Subject, "can", Verb, EachOther })
            .Action(() =>
            {
                var verb = Verb.Verb;
                verb.SubjectKind = verb.ObjectKind = Subject.CommonNoun;
                verb.IsSymmetric = true;
            })
            .Check(VerbBaseForm, SubjectCommonNoun),

        new Syntax(() => new object[] { Subject, Is, "a", "kind", "of", Object })
            .Action(() =>
            {
                Subject.CommonNoun.DeclareSuperclass(Object.CommonNoun);
                foreach (var mod in Object.Modifiers)
                    Subject.CommonNoun.ImpliedAdjectives.Add(new CommonNoun.ConditionalAdjective(null, mod));
            })
            .Check(SubjectVerbAgree, ObjectSingular, SubjectUnmodified, SubjectCommonNoun, ObjectCommonNoun)
            .Documentation("Declares that all Subjects are also Objects.  For example, 'cat is a kind of animal' says anythign that is a cat is also an animal."),

        new Syntax(() => new object[] { SubjectNounList, Is, "kinds", "of", Object })
            .Action(() =>
            {
                foreach (var noun in SubjectNounList.Concepts)
                {
                    var c = noun as CommonNoun;
                    if (c == null)
                        throw new GrammaticalError("This noun is a proper noun (a name of a specific thing), but I need a common noun (a kind of thing) here", noun.StandardName);
                    c.DeclareSuperclass(Object.CommonNoun);
                    foreach (var mod in Object.Modifiers)
                        c.ImpliedAdjectives.Add(new CommonNoun.ConditionalAdjective(null, mod));

                }
            })
            .Check(ObjectSingular, ObjectCommonNoun)
            .Documentation("Declares that all the different nouns in the subject list are also kinds of the object noun.  So 'dogs and cats are kinds of animal' states that all dogs and all cats are also animals."),

        new Syntax(() => new object[] { "the", "plural", "of", Subject, "is", Object })
            .Action(() =>
            {
                Subject.Number = Number.Singular;
                Object.Number = Number.Plural;
                Subject.CommonNoun.PluralForm = Object.Text;
            })
            .Check(SubjectUnmodified, ObjectUnmodified, SubjectCommonNoun, ObjectCommonNoun)
            .Documentation("Lets you correct the system's guess as to the plural of a noun."),

        new Syntax(() => new object[] { "the", "singular", "of", Subject, "is", Object })
            .Action(() =>
            {
                Subject.Number = Number.Plural;
                Object.Number = Number.Singular;
                Subject.CommonNoun.SingularForm = Object.Text;
            })
            .Check(SubjectUnmodified, ObjectUnmodified, SubjectCommonNoun, ObjectCommonNoun)
            .Documentation("Lets you correct the system's guess as to the singular of a noun."),

        new Syntax(() => new object[] { Subject, Is, "identified", "as", "\"", Text, "\"" })
            .Action( () => Subject.CommonNoun.NameTemplate = Text.Text)
            .Check(SubjectUnmodified, SubjectCommonNoun)
            .Documentation("Tells the system how to print the name of an object."),

        new Syntax(() => new object[] { Subject, "can", "be", PredicateAP })
            .Action(() =>
            {
                if (!PredicateAP.Adjective.RelevantTo(Subject.CommonNoun))
                    Subject.CommonNoun.RelevantAdjectives.Add(PredicateAP.Adjective);
            })
            .Check(SubjectUnmodified)
            .Documentation("Declares that Subjects can be Adjectives, but don't have to be."),

        new Syntax(() => new object[] { Subject, "is", Object })
            .Action(() =>
            {
                var proper = (ProperNoun) Subject.Noun;
                proper.Kinds.Add(Object.CommonNoun);
            })
            .Check(SubjectProperNoun, ObjectCommonNoun),
        //new Syntax(() => new object[] { Subject, Is, Object })
        //    .Action(() =>
        //    {
        //        var n = (CommonNoun) Subject.Noun;
        //        n.ImpliedAdjectives.Add(new CommonNoun.ConditionalAdjective(Subject.Modifiers.ToArray(), Object.CommonNoun));
        //    })
        //    .Check(SubjectCommonNoun, ObjectCommonNoun, SubjectVerbAgree),
        new Syntax(() => new object[] { Subject, Is, PredicateAP })
            .Action(() =>
            {
                Subject.CommonNoun.ImpliedAdjectives.Add(new CommonNoun.ConditionalAdjective(Subject.Modifiers.ToArray(), PredicateAP.Adjective));
            })
            .Check(SubjectVerbAgree)
            .Documentation("Declares that Subjects are always Adjective.  For example, 'cats are fuzzy' declares that all cats are also fuzzy."),

        new Syntax(() => new object[] { Subject, Is, PredicateAPList })
            .Action(() =>
            {
                    Subject.CommonNoun.AlternativeSets.Add(new CommonNoun.AlternativeSet(PredicateAPList.Concepts.ToArray(), true));
            })
            .Check(SubjectVerbAgree, SubjectUnmodified)
            .Documentation("Declares that Subjects must be one of the Adjectives.  So 'cats are big or small' says cats are always either big or small, but not both or neither."),

        new Syntax(() => new object[] { Subject, "can", "be", PredicateAPList })
            .Action(() =>
            {
                Subject.CommonNoun.AlternativeSets.Add(new CommonNoun.AlternativeSet(PredicateAPList.Concepts.ToArray(), false));
            })
            .Check(SubjectDefaultPlural, SubjectUnmodified)
            .Documentation("Declares that Subjects can be any one of the Adjectives, but don't have to be.  So 'cats can be big or small' says cats can be big, small, or neither, but not both."),

        new Syntax(() => new object[] { Subject, Has, Object, "between", LowerBound, "and", UpperBound })
            .Action(() =>
                {
                    Subject.CommonNoun.Properties.Add(new Property(Object.Text, new FloatDomain(Object.Text.Untokenize(), lowerBound, upperBound)));
                })
            .Check(SubjectVerbAgree, SubjectUnmodified, ObjectUnmodified)
            .Documentation("Says Subjects have a property, Object, that is a number in the specified range.  For example, 'cats have an age between 1 and 20'"),
        
        new Syntax(() => new object[] { Subject, Has, Object, "from", ListName })
            .Action(() =>
            {
                var menuName = ListName.Text.Untokenize();
                var menu = new Menu<string>(menuName, File.ReadAllLines(DefinitionFilePath(menuName)));
                var propertyName = Object.Text;
                var prop = Subject.CommonNoun.Properties.FirstOrDefault(p => p.IsNamed(propertyName));
                if (prop == null)
                {
                    prop = new Property(propertyName, null);
                    Subject.CommonNoun.Properties.Add(prop);
                }

                prop.MenuRules.Add(new Property.MenuRule(Subject.Modifiers.ToArray(), menu));
            })
            .Check(SubjectVerbAgree, ObjectUnmodified)
            .Documentation("Say Subjects have a property whose possible values are given in the specified file.  For example 'cats have a name from cat names', or 'French cats have a name from French cat names'"),
    };

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
        if (VerbNumber == Number.Singular)
            return false;
        VerbNumber = Number.Plural;
        return true;
    }

    private static bool SubjectDefaultPlural()
    {
        if (Subject.Number == null)
            Subject.Number = Number.Plural;
        return true;
    }

    private static bool SubjectPlural() => Subject.Number == Number.Plural;

    //private static bool ObjectNonSingular() => Object.Number == null || Object.Number == Number.Plural;

    private static bool ObjectSingular()
    {
        if (Object.Number == Number.Plural)
            throw new GrammaticalError("Noun should be in singular form", Object.Text);

        Object.Number = Number.Singular;
        return true;
    }

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
            throw new GrammaticalError("Subject noun cannot take adjectives", Subject.Text);
        return true;
    }

    /// <summary>
    /// Used for sentential forms that can't accept adjectives in their object.
    /// </summary>
    /// <returns></returns>
    private static bool ObjectUnmodified()
    {
        if (Object.Modifiers.Count > 0)
            throw new GrammaticalError("Object noun cannot take adjectives", Subject.Text);
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
    /// Try to make a syntax rule and run its action if successful.
    /// </summary>
    /// <returns>True on success</returns>
    public bool Try()
    {
        ResetParser();
        var old = State;

        if (MatchConstituents() && EndOfInput && (validityTests == null || validityTests.All(test => test())))
        {
            action();
            return true;
        }
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
        for (int i = 0; i < constituents.Length; i++)
        {
            var c = constituents[i];
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
                    else if (next is ClosedClassSegment s)
                    {
                        if (!seg.ScanTo(s.IsPossibleStart))
                            return false;
                    }
                    else if (seg is ClosedClassSegment)
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
            }
            else if (c is Func<bool> test)
            {
                if (!test())
                    return false;
            }
            else throw new ArgumentException($"Unknown type of constituent {c}");

        }

        return true;
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

    public Syntax DebugMatch()
    {
        BreakOnMatch = true;
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
    private string docString;

    /// <summary>
    /// Adds the specified documentation string to the Syntax form.
    /// </summary>
    public Syntax Documentation(string doc)
    {
        docString = doc;
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

            Buffer.Append("</b>\n    ");
            Buffer.Append(docString??"");
            Buffer.Append('\n');
            return Buffer.ToString();
        }
    }

    private static string ConstituentName(object c)
    {
        switch (c)
        {
            case string s:
                return s;

            case Segment seg:
                return $"<i>{seg.Name}</i>";

            case Func<bool> f:
                if (f == Is)
                    return "is/are";
                if (f == Has)
                    return "have/has";
                if (f == LowerBound)
                    return "<i>LowerBound</i>";
                if (f == UpperBound)
                    return "<i>UpperBound</i>";
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