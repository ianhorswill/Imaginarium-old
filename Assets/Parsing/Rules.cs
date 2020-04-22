#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rules.cs" company="Ian Horswill">
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
using static Parser;
using System.IO;
using System.Linq;
using CatSAT.NonBoolean.SMT.Float;
using CatSAT.NonBoolean.SMT.MenuVariables;

/// <summary>
/// Rules for parsing the top-level syntax of sentences.
/// </summary>
public partial class Syntax
{
    /// <summary>
    /// Rules for the different sentential forms understood by the system.
    /// Each consists of a pattern to recognize the form and store its components in static fields such
    /// as Subject, Object, and VerbNumber, and an Action to perform updates to the ontology based on
    /// the data stored in those static fields.  The Check option is used to insure features are properly
    /// set.
    /// </summary>
    public static readonly Syntax[] AllRules =
    {
        new Syntax("debug")
            .Action( () => Syntax.LogAllParsing = !Syntax.LogAllParsing)
            .Documentation("Toggles debugging of input parsing")
            .Command(), 

        new Syntax("help")
            .Action(() =>
            {
                foreach (var r in AllRules) 
                    Driver.AppendResponseLine(r.HelpDescription);
            })
            .Documentation("Prints this list of commands")
            .Command(),

        new Syntax("quit")
            .Action(UnityEngine.Application.Quit)
            .Documentation("Ends the application")
            .Command(),

        new Syntax("exit")
            .Action(UnityEngine.Application.Quit)
            .Documentation("Ends the application")
            .Command(),

        new Syntax(() => new object[] { "pressing", "!", "\"", ButtonName, "\"", "means", "\"", Text, "\"" })
            .Action(() =>
            {
                Driver.Repl.AddButton(ButtonName.Text.Untokenize(), Text.Text.Untokenize());
            })
            .Documentation("Instructs the system to add a new button to the button bar with the specified name.  When it is pressed, it will execute the specified text."),

        new Syntax(() => new object[] { "imagine", "!", Object })
            .Action(() =>
            {
                var countRequest = Object.ExplicitCount;
                var count = countRequest ?? (Object.Number == Number.Plural?9:1);
                Generator.Current = new Generator(Object.CommonNoun, Object.Modifiers, count);
            })
            .Command()
            .Documentation("Generates one or more Objects.  For example, 'imagine a cat' or 'imagine 10 long-haired cats'."),

        new Syntax("undo")
            .Action( () => History.Undo() )
            .Command()
            .Documentation("Undoes the last change to the ontology."), 

        new Syntax("start", "over")
            .Action( () =>
            {
                History.Clear();
                Driver.AppendResponseLine("Knowledge-base erased.  I don't know anything.");
            })
            .Command()
            .Documentation("Tells the system to forget everything you've told it about the world."), 
        
        new Syntax(() => new object[] { "save", ListName })
            .Action(() =>
            {
                History.Save(Parser.DefinitionFilePath(ListName.Text.Untokenize()));
            })
            .Command()
            .Documentation("Saves assertions to a file."),

        //new Syntax(() => new object[] { "edit", ListName })
        //    .Action(() =>
        //    {
        //        var definitionFilePath = Parser.DefinitionFilePath(ListName.Text.Untokenize());

        //        Ontology.EraseConcepts();
        //        Parser.LoadDefinitions(definitionFilePath);
        //        History.Edit(definitionFilePath);
        //    })
        //    .Command()
        //    .Documentation("Add new assertions to a file.  Use save command to save changes."),
        
        new Syntax(() => new object[] { OptionalAll, Subject, CanMust, Verb, Quantifier, "!", Object })
            .Action(() =>
            {
                var verb = Verb.Verb;
                verb.SubjectKind = CommonNoun.LeastUpperBound(verb.SubjectKind, Subject.CommonNoun);
                verb.ObjectKind = CommonNoun.LeastUpperBound(verb.ObjectKind, Object.CommonNoun);
                verb.IsFunction |= !Quantifier.IsPlural;
                // "Cats can love other cats" means anti-reflexive, whereas "cats can love many cats" doesn't.
                verb.IsAntiReflexive |= Quantifier.IsOther;
                verb.IsTotal |= CanMust.Match[0] == "must";
            })
            .Check(VerbBaseForm, ObjectUnmodified, ObjectQuantifierAgree, SubjectCommonNoun, ObjectCommonNoun)
            .Documentation("Specifies how many Objects a given Subject can Verb."),

        new Syntax(() => new object[] { Verb, "is", RareCommon })
            .Action(() => Verb.Verb.Density = RareCommon.Value)
            // ReSharper disable once StringLiteralTypo
            .Documentation("States that Verb'ing is rare or common."), 

        new Syntax(() => new object[] { Verb, "and", Verb2, "are", "!", "mutually", "exclusive" })
            .Action(() =>
            {
                Verb.Verb.MutualExclusions.Add(Verb2.Verb);
            })
            .Documentation("States that two objects cannot be related by both verbs at once."),
        
        new Syntax(() => new object[] { Verb, "is", "mutually", "!", "exclusive", "with", Verb2 })
            .Action(() =>
            {
                Verb.Verb.MutualExclusions.Add(Verb2.Verb);
            })
            .Documentation("States that two objects cannot be related by both verbs at once."),

        new Syntax(() => new object[] { Verb, "implies", "!", Verb2 })
            .Action(() =>
            {
                Verb.Verb.Generalizations.Add(Verb2.Verb);
            })
            .Check(VerbGerundForm, Verb2GerundForm)
            .Documentation("States that two objects being related by the first verb means they must also be related by the second."), 

        new Syntax(() => new object[] { Verb, "is", "a", "way", "!", "of", Verb2 })
            .Action(() =>
            {
                if (Verb.Verb.SubjectKind == null)
                    Verb.Verb.SubjectKind = Verb2.Verb.SubjectKind;
                if (Verb.Verb.ObjectKind == null)
                    Verb.Verb.ObjectKind = Verb2.Verb.ObjectKind;

                if (Verb2.Verb.SubjectKind == null)
                    Verb2.Verb.SubjectKind = Verb.Verb.SubjectKind;
                if (Verb2.Verb.ObjectKind == null)
                    Verb2.Verb.ObjectKind = Verb.Verb.ObjectKind;

                Verb.Verb.Superspecies.Add(Verb2.Verb);
                Verb2.Verb.Subspecies.Add(Verb.Verb);
            })
            .Check(VerbGerundForm, Verb2GerundForm)
            .Documentation("Like 'is a kind of' but for verbs.  Verb1 implies Verb2, but Verb2 implies that one of the Verbs that is a way of Verb2ing is also true."), 

        new Syntax(() => new object[] { Subject, "are", RareCommon })
            .Action(() => Subject.CommonNoun.InitialProbability = RareCommon.Value)
            .Check(SubjectPlural, SubjectUnmodified)
            .Documentation("States that the specified kind of object is rare/common."), 

        new Syntax(() => new object[] { OptionalAll, Subject, CanNot, Verb, Reflexive })
            .Action(() =>
            {
                var verb = Verb.Verb;
                verb.SubjectKind = verb.ObjectKind = CommonNoun.LeastUpperBound(verb.SubjectKind, verb.ObjectKind, Subject.CommonNoun);
                verb.IsAntiReflexive = true;
            })
            .Check(VerbBaseForm, SubjectCommonNoun)
            .Documentation("States that the verb can't hold between an object and itself."),

        new Syntax(() => new object[] { OptionalAll, Subject, Always, Verb, Reflexive })
            .Action(() =>
            {
                var verb = Verb.Verb;
                verb.SubjectKind = verb.ObjectKind = CommonNoun.LeastUpperBound(verb.SubjectKind, verb.ObjectKind, Subject.CommonNoun);
                verb.IsReflexive = true;
            })
            .Check(VerbBaseForm, SubjectCommonNoun)
            .Documentation("States that the verb always holds between objects and themselves."),

        new Syntax(() => new object[] { OptionalAll, Subject, "can", Verb, EachOther })
            .Action(() =>
            {
                var verb = Verb.Verb;
                verb.SubjectKind = verb.ObjectKind = CommonNoun.LeastUpperBound(verb.SubjectKind, verb.ObjectKind, Subject.CommonNoun);
                verb.IsSymmetric = true;
            })
            .Check(VerbBaseForm, SubjectCommonNoun)
            .Documentation("States that the verb is symmetric: if a verbs b, then b verbs a."),

        new Syntax(() => new object[] { Subject, Is, "a", "kind", "!", "of", Object })
            .Action(() =>
            {
                Subject.CommonNoun.DeclareSuperclass(Object.CommonNoun);
                foreach (var mod in Object.Modifiers)
                    Subject.CommonNoun.ImpliedAdjectives.Add(new CommonNoun.ConditionalModifier(null, mod));
            })
            .Check(SubjectVerbAgree, ObjectSingular, SubjectUnmodified, SubjectCommonNoun, ObjectCommonNoun)
            .Documentation("Declares that all Subjects are also Objects.  For example, 'cat is a kind of animal' says anything that is a cat is also an animal."),

        new Syntax(() => new object[] { SubjectNounList, Is, "kinds", "!", "of", Object })
            .Action(() =>
            {
                foreach (var noun in SubjectNounList.Concepts)
                {
                    var c = noun as CommonNoun;
                    if (c == null)
                        throw new GrammaticalError($"The noun '{noun.StandardName}' is a proper noun (a name of a specific thing), but I need a common noun (a kind of thing) here",
                            $"The noun '<i>{noun.StandardName}</i>' is a proper noun (a name of a specific thing), but I need a common noun (a kind of thing) here");
                    c.DeclareSuperclass(Object.CommonNoun);
                    foreach (var mod in Object.Modifiers)
                        c.ImpliedAdjectives.Add(new CommonNoun.ConditionalModifier(null, mod));

                }
            })
            .Check(ObjectSingular, ObjectCommonNoun)
            .Documentation("Declares that all the different nouns in the subject list are also kinds of the object noun.  So 'dogs and cats are kinds of animal' states that all dogs and all cats are also animals."),

        new Syntax(() => new object[] { "the", "plural", "!", "of", Subject, "is", Object })
            .Action(() =>
            {
                Subject.Number = Number.Singular;
                Object.Number = Number.Plural;
                Subject.CommonNoun.PluralForm = Object.Text;
            })
            .Check(SubjectUnmodified, ObjectUnmodified, SubjectCommonNoun, ObjectCommonNoun)
            .Documentation("Lets you correct the system's guess as to the plural of a noun."),

        new Syntax(() => new object[] { "the", "singular", "!", "of", Subject, "is", Object })
            .Action(() =>
            {
                Subject.Number = Number.Plural;
                Object.Number = Number.Singular;
                Subject.CommonNoun.SingularForm = Object.Text;
            })
            .Check(SubjectUnmodified, ObjectUnmodified, SubjectCommonNoun, ObjectCommonNoun)
            .Documentation("Lets you correct the system's guess as to the singular form of a noun."),

        new Syntax(() => new object[] { Subject, Is, "identified", "!", "as", "\"", Text, "\"" })
            .Action( () => Subject.CommonNoun.NameTemplate = Text.Text)
            .Check(SubjectUnmodified, SubjectCommonNoun)
            .Documentation("Tells the system how to print the name of an object."),

        new Syntax(() => new object[] { Subject, Is, "described", "!", "as", "\"", Text, "\"" })
            .Action( () => Subject.CommonNoun.DescriptionTemplate = Text.Text)
            .Check(SubjectUnmodified, SubjectCommonNoun)
            .Documentation("Tells the system how to generate the description of an object."),

        new Syntax(() => new object[] { OptionalAll, Subject, "can", "be", PredicateAP })
            .Action(() =>
            {
                if (!PredicateAP.Adjective.RelevantTo(Subject.CommonNoun))
                    Subject.CommonNoun.RelevantAdjectives.Add(PredicateAP.Adjective);
            })
            .Check(SubjectUnmodified)
            .Documentation("Declares that Subjects can be Adjectives, but don't have to be."),

        new Syntax(() => new object[] { "Do", "not", "mention", "!", "being", PredicateAP })
            .Action(() => { PredicateAP.Adjective.IsSilent = true; })
            .Documentation("Declares that the specified adjective shouldn't be mentioned in descriptions."),

        new Syntax(() => new object[] { Subject, "is", Object })
            .Action(() =>
            {
                var proper = (ProperNoun) Subject.Noun;
                proper.Kinds.Add(Object.CommonNoun);
            })
            .Check(SubjectProperNoun, ObjectCommonNoun, ObjectExplicitlySingular)
            .Documentation("States that proper noun Subject is of the type Object.  For example, 'Ben is a person'."),

        new Syntax(() => new object[] { Subject, "is", OptionalAlways, Object })
            .Action(() =>
            {
                var subject = Subject.CommonNoun;
                subject.ImpliedAdjectives.Add(new CommonNoun.ConditionalModifier(Subject.Modifiers.ToArray(),
                    Object.CommonNoun));
            })
            .Check(SubjectCommonNoun, ObjectCommonNoun, ObjectExplicitlySingular)
            .Documentation("States that a Subject is always also a Object.  Importnat: object *must* be singular, as in 'a noun', not just 'nouns'.  This is different from saying 'a kind of' which says that Objects must also be one of their subkinds.  This just says if you see a Subject, also make it be an Object."),

        //new Syntax(() => new object[] { Subject, Is, Object })
        //    .Action(() =>
        //    {
        //        var n = (CommonNoun) Subject.Noun;
        //        n.ImpliedAdjectives.Add(new CommonNoun.ConditionalAdjective(Subject.Modifiers.ToArray(), Object.CommonNoun));
        //    })
        //    .Check(SubjectCommonNoun, ObjectCommonNoun, SubjectVerbAgree),
        new Syntax(() => new object[] { OptionalAll, Subject, Is, PredicateAP })
            .Action(() =>
            {
                switch (Subject.Noun)
                {
                    case CommonNoun c:
                        c.ImpliedAdjectives.Add(new CommonNoun.ConditionalModifier(Subject.Modifiers.ToArray(),
                            PredicateAP.MonadicConceptLiteral));
                        break;

                    case ProperNoun n:
                        n.Individual.Modifiers.Add(PredicateAP.MonadicConceptLiteral);
                        break;

                    default:
                        throw new Exception(
                            $"Unknown kind of noun ({Subject.Noun.GetType().Name}: '{Subject.Noun.StandardName.Untokenize()}'");
                }
            })
            .Check(SubjectVerbAgree)
            .Documentation("Declares that Subjects are always Adjective.  For example, 'cats are fuzzy' declares that all cats are also fuzzy."),

        new Syntax(() => new object[] { OptionalAll, Subject, Is, "any", "!", LowerBound, "of", PredicateAPList })
            .Action(() =>
            {
                var alternatives = PredicateAPList.Expressions.Select(ap => ap.MonadicConceptLiteral).ToArray();
                var alternativeSet = new CommonNoun.AlternativeSet(alternatives, (int)lowerBound, (int)lowerBound);
                Subject.CommonNoun.AlternativeSets.Add(alternativeSet);
            })
            .Check(SubjectVerbAgree, SubjectUnmodified)
            .Documentation("Declares the specified number of Adjectives must be true of Subjects."),
        
        new Syntax(() => new object[] { OptionalAll, Subject, Is, "between", "!", LowerBound, "and", UpperBound, "of", PredicateAPList })
            .Action(() =>
            {
                var alternatives = PredicateAPList.Expressions.Select(ap => ap.MonadicConceptLiteral).ToArray();
                var alternativeSet = new CommonNoun.AlternativeSet(alternatives, (int)lowerBound, (int)upperBound);
                Subject.CommonNoun.AlternativeSets.Add(alternativeSet);
            })
            .Check(SubjectVerbAgree, SubjectUnmodified)
            .Documentation("Declares the number of Adjectives true of Subjects must be in the specified range."),
        
        new Syntax(() => new object[] { OptionalAll, Subject, "can", "be", "up", "!", "to", LowerBound, "of", PredicateAPList })
            .Action(() =>
            {
                var alternatives = PredicateAPList.Expressions.Select(ap => ap.MonadicConceptLiteral).ToArray();
                var alternativeSet = new CommonNoun.AlternativeSet(alternatives, 0, (int)lowerBound);
                Subject.CommonNoun.AlternativeSets.Add(alternativeSet);
            })
            .Check(SubjectVerbAgree, SubjectUnmodified)
            .Documentation("Declares the number of Adjectives true of Subjects can never be more than the specified number."),

        new Syntax(() => new object[] { OptionalAll, Subject, Is, PredicateAPList })
            .Action(() =>
            {
                    Subject.CommonNoun.AlternativeSets.Add(new CommonNoun.AlternativeSet(PredicateAPList.Expressions.Select(ap => ap.MonadicConceptLiteral).ToArray(), true));
            })
            .Check(SubjectVerbAgree, SubjectUnmodified)
            .Documentation("Declares that Subjects must be one of the Adjectives.  So 'cats are big or small' says cats are always either big or small, but not both or neither."),

        new Syntax(() => new object[] { OptionalAll, Subject, "can", "be", PredicateAPList })
            .Action(() =>
            {
                Subject.CommonNoun.AlternativeSets.Add(new CommonNoun.AlternativeSet(PredicateAPList.Expressions.Select(ap => ap.MonadicConceptLiteral).ToArray(), false));
            })
            .Check(SubjectDefaultPlural, SubjectUnmodified)
            .Documentation("Declares that Subjects can be any one of the Adjectives, but don't have to be.  So 'cats can be big or small' says cats can be big, small, or neither, but not both."),

        new Syntax(() => new object[] { OptionalAll, Subject, Has, Object, "between", "!", LowerBound, "and", UpperBound })
            .Action(() =>
                {
                    Subject.CommonNoun.Properties.Add(new Property(Object.Text, new FloatDomain(Object.Text.Untokenize(), lowerBound, upperBound)));
                })
            .Check(SubjectVerbAgree, SubjectUnmodified, ObjectUnmodified)
            .Documentation("Says Subjects have a property, Object, that is a number in the specified range.  For example, 'cats have an age between 1 and 20'"),
        
        new Syntax(() => new object[] { OptionalAll, Subject, Has, Object, "from", "!", ListName })
            .Action(() =>
            {
                var menuName = ListName.Text.Untokenize();
                if (!NameIsValidFilename(menuName))
                    throw new Exception($"The list name \"{menuName}\" is not a valid file name.");
                var menu = new Menu<string>(menuName, File.ReadAllLines(ListFilePath(menuName)));
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
            .Documentation("States that Subjects have a property whose possible values are given in the specified file.  For example 'cats have a name from cat names', or 'French cats have a name from French cat names'"),

        new Syntax(() => new object[] { OptionalAll, Subject, Has, Object, "called", "!", "its", Text })
            .Action(() =>
            {

                var partName = Text.Text;
                var part = Subject.CommonNoun.Parts.FirstOrDefault(p => p.IsNamed(partName));
                if (part == null)
                {
                    part = new Part(partName, Object.CommonNoun, Object.Modifiers);
                    Subject.CommonNoun.Parts.Add(part);
                }
            })
            .Check(SubjectVerbAgree, ObjectUnmodified)
            .Documentation("States that Subjects have part called Text that is a Object."),

        new Syntax(() => new object[] { Subject, "should", "!", ExistNotExist})
            .Action(() =>
            {
                var shouldExist = ExistNotExist.Match[0] == "exist";
                var input = Input.Untokenize();
                Ontology.AddTest(Subject.CommonNoun, Subject.Modifiers, shouldExist,
                    $"Test succeeded: {input}",
                    $"Test failed: {input}");
            })
            .Documentation("Adds a new test to the list of tests to perform when the test command is used."),
        
        new Syntax("test")
            .Action(() =>
            {
                var gotOne = false;
                foreach (var (test, success, example) in Ontology.TestResults())
                {
                    gotOne = true;
                    Driver.AppendResponseLine(
                        success?
                            $"<B><color=green>{test.SucceedMessage}</color></b>"
                            :$"<b><color=red>{test.FailMessage}</color></b>"
                        );
                    if (example != null)
                        Driver.AppendResponseLine($"Example: {example.Description(example.Individuals[0])}");
                }

                if (!gotOne)
                    Driver.AppendResponseLine("No tests have been defined.");
            })
            .Documentation("Run all tests currently defined")
            .Command(), 
    };
}