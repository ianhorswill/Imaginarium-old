using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

/// <summary>
/// A noun that represents a kind of thing
/// </summary>
[DebuggerDisplay("{" + nameof(Text) + "}")]
public class CommonNoun : Noun
{
    /// <summary>
    /// List of all common nouns (i.e. kinds/types) in the ontology.
    /// </summary>
    public static IEnumerable<CommonNoun> AllCommonNouns => AllNouns.Select(pair => pair.Value)
        .Where(n => n is CommonNoun).Cast<CommonNoun>().Distinct();

    /// <inheritdoc />
    public override bool IsNamed(string[] tokens)
    {
        return SingularForm.SameAs(tokens) || PluralForm.SameAs(tokens);
    }

    // ReSharper disable InconsistentNaming
    private string[] _singular, _plural;
    // ReSharper restore InconsistentNaming

    /// <summary>
    /// Singular form of the noun
    /// </summary>
    public string[] SingularForm
    {
        get
        {
            EnsureSingularForm();
            return _singular;
        }
        set
        {
            if (_singular != null)
            {
                AllNouns.Remove(_singular);
                Store(_singular, null);
            }
            _singular = value;
            AllNouns[_singular] = this;
            Store(_singular, this);
            EnsurePluralForm();
        }
    }

    /// <summary>
    /// Make sure the noun has a singular form.
    /// </summary>
    private void EnsureSingularForm()
    {
        if (_singular == null)
            SingularForm = Inflection.SingularOfNoun(_plural);
    }

    /// <summary>
    /// Plural form of the noun, for common nouns
    /// </summary>
    public string[] PluralForm
    {
        get
        {
            EnsurePluralForm();
            return _plural;
        }
        set
        {
            if (_plural != null)
            {
                AllNouns.Remove(_plural);
                Store(_plural, null);
            }
            _plural = value;
            AllNouns[_plural] = this;
            Store(_plural, this, true);
            EnsureSingularForm();
        }
    }

    private void EnsurePluralForm()
    {
        if (_plural == null)
            PluralForm = Inflection.PluralOfNoun(_singular);
    }

    /// <inheritdoc />
    public override string[] StandardName => SingularForm ?? PluralForm;

    /// <summary>
    /// Template used to generate a reference to the object
    /// </summary>
    public string[] NameTemplate { get; set; }

    /// <summary>
    /// Returns the common noun identified by the specified sequence of tokens, or null, if there is no such noun.
    /// </summary>
    /// <param name="name">Tokens identifying the noun</param>
    /// <returns>Identified noun, or null if none found.</returns>
    public new static CommonNoun Find(params string[] name)
    {
        return (CommonNoun)Noun.Find(name);
    }

    /// <summary>
    /// The common nouns identifying the subtypes of this noun
    /// </summary>
    public readonly List<CommonNoun> Subkinds = new List<CommonNoun>();
    /// <summary>
    /// The common nouns identifying the superkinds of this noun
    /// </summary>
    public readonly List<CommonNoun> Superkinds = new List<CommonNoun>();
    /// <summary>
    /// Adjectives might apply to this kind of noun.
    /// Relevant adjectives of super- and subkinds might also apply but not be listed in this list.
    /// </summary>
    public readonly List<Adjective> RelevantAdjectives = new List<Adjective>();
    /// <summary>
    /// Sets of mutually exclusive concepts that apply to this kind of object
    /// </summary>
    public readonly List<AlternativeSet> AlternativeSets = new List<AlternativeSet>();
    /// <summary>
    /// Adjectives that are always true of this kind of object.
    /// </summary>
    public readonly List<ConditionalAdjective> ImpliedAdjectives = new List<ConditionalAdjective>();
    /// <summary>
    /// Properties attached to this kind of object
    /// Objects of this kind may also have properties attached to sub- and superkinds.
    /// </summary>
    public readonly List<Property> Properties = new List<Property>();

    /// <summary>
    /// Return the property of this noun with the specified name, or null.
    /// </summary>
    public Property PropertyNamed(string[] name)
    {
        return Properties.FirstOrDefault(p => p.IsNamed(name));
    }

    public void ForAllAncestorKinds(Action<CommonNoun> a, bool includeSelf = true)
    {
        if (includeSelf)
            a(this);
        foreach (var super in Superkinds)
            super.ForAllAncestorKinds(a);
    }

    public void ForAllDescendantKinds(Action<CommonNoun> a, bool includeSelf = true)
    {
        if (includeSelf)
            a(this);
        foreach (var super in Subkinds)
            super.ForAllDescendantKinds(a);
    }

    public bool IsImmediateSuperKindOf(CommonNoun super) => Subkinds.Contains(super);
    public bool IsImmediateSubKindOf(CommonNoun sub) => Superkinds.Contains(sub);

    /// <summary>
    /// Ensure super is an immediate super-kind of this kind.
    /// Does nothing if it is already a super-kind.
    /// </summary>
    public void DeclareSuperclass(CommonNoun super)
    {
        if (!Superkinds.Contains(super))
        {
            Superkinds.Add(super);
            super.Subkinds.Add(this);
        }
    }

    /// <summary>
    /// A set of mutually exclusive adjectives that can apply to a CommonNoun.
    /// </summary>
    public struct AlternativeSet
    {
        /// <summary>
        /// At most one of these may be true of the noun
        /// </summary>
        public readonly Adjective[] Alternatives;
        /// <summary>
        /// Whether one of them *has* to be true of the noun
        /// </summary>
        public readonly bool IsRequired;

        public AlternativeSet(Adjective[] alternatives, bool isRequired)
        {
            Alternatives = alternatives;
            IsRequired = isRequired;
        }
    }

    /// <summary>
    /// An adjective together with an optional list of modifiers that allow it to apply
    /// </summary>
    public class ConditionalAdjective
    {
        private static readonly MonadicConcept[] EmptyCondition = new MonadicConcept[0];

        /// <summary>
        /// Additional conditions on top of the CommonNoun in which this is stored, that must be true for the implication to hold
        /// </summary>
        public readonly MonadicConcept[] Conditions;
        /// <summary>
        /// Adjective that follows from the noun and conditions.
        /// </summary>
        public readonly MonadicConcept Adjective;

        public ConditionalAdjective(MonadicConcept[] conditions, MonadicConcept adjective)
        {
            Conditions = conditions??EmptyCondition;
            Adjective = adjective;
        }
    }
}
