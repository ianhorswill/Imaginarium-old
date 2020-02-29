#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonNoun.cs" company="Ian Horswill">
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
using System.Linq;

/// <summary>
/// A noun that represents a kind of thing
/// </summary>
[DebuggerDisplay("{" + nameof(Text) + "}")]
public class CommonNoun : Noun
{
    public CommonNoun() : base(null)
    { }

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
            if (_singular != null && ((TokenString) _singular).Equals((TokenString) value))
                return;
            // If we're defining the plural to be identical to the singular, don't check for name collision
            if (_plural == null || !((TokenString) _plural).Equals((TokenString) value))
                Ontology.EnsureUndefinedOrDefinedAsType(value, GetType());
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
            if (_plural != null && ((TokenString) _plural).Equals((TokenString) value))
                return;
            // If we're defining the plural to be identical to the singular, don't check for name collision
            if (_singular == null || !((TokenString) _singular).Equals((TokenString) value))
                Ontology.EnsureUndefinedOrDefinedAsType(value, GetType());
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
    public readonly List<ConditionalModifier> ImpliedAdjectives = new List<ConditionalModifier>();
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

    public bool IsImmediateSuperKindOf(CommonNoun sub) => Subkinds.Contains(sub);
    public bool IsImmediateSubKindOf(CommonNoun super) => Superkinds.Contains(super);

    public bool IsSuperKindOf(CommonNoun sub) => sub == this || Subkinds.Any(IsSuperKindOf);

    public bool IsSubKindOf(CommonNoun super) => super.IsSuperKindOf(this);

    public static CommonNoun LeastUpperBound(CommonNoun a, CommonNoun b)
    {
        if (a == null)
            return b;
        if (b == null)
            return a;

        if (a.IsSuperKindOf(b))
            return a;
        
        foreach (var super in a.Superkinds)
        {
            var lub = LeastUpperBound(super, b);
            if (lub != null)
                return lub;
        }

        return null;
    }

    public static CommonNoun LeastUpperBound(CommonNoun a, CommonNoun b, CommonNoun c) =>
        a == null ? LeastUpperBound(b, c) : LeastUpperBound(a, LeastUpperBound(b, c));

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
        public readonly MonadicConceptLiteral[] Alternatives;
        /// <summary>
        /// Whether one of them *has* to be true of the noun
        /// </summary>
        public readonly bool IsRequired;

        public AlternativeSet(MonadicConceptLiteral[] alternatives, bool isRequired)
        {
            Alternatives = alternatives;
            IsRequired = isRequired;
        }
    }

    /// <summary>
    /// An adjective together with an optional list of modifiers that allow it to apply
    /// </summary>
    public class ConditionalModifier
    {
        private static readonly MonadicConceptLiteral[] EmptyCondition = new MonadicConceptLiteral[0];

        /// <summary>
        /// Additional conditions on top of the CommonNoun in which this is stored, that must be true for the implication to hold
        /// </summary>
        public readonly MonadicConceptLiteral[] Conditions;
        /// <summary>
        /// Adjective that follows from the noun and conditions.
        /// </summary>
        public readonly MonadicConceptLiteral Modifier;

        public ConditionalModifier(MonadicConceptLiteral[] conditions, MonadicConceptLiteral modifier)
        {
            Conditions = conditions??EmptyCondition;
            Modifier = modifier;
        }
    }
}
