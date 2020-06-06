#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Verb.cs" company="Ian Horswill">
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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Represents a verb, i.e. a binary relation
/// </summary>
public class Verb : Concept
{
    public Verb(Ontology ontology) : base(ontology, null)
    { }

    public override string Description
    {
        get
        {
            var b = new StringBuilder();
            b.Append(base.Description);
            b.Append(
                $" ({SingularForm.Untokenize()}/{PluralForm.Untokenize()}/{GerundForm.Untokenize()}/is {PassiveParticiple.Untokenize()} by)\n");
            if (IsReflexive || IsAntiReflexive || IsSymmetric || IsAntiSymmetric)
            {
                if (IsReflexive)
                    b.Append("reflexive ");
                if (IsAntiReflexive)
                    b.Append("anti-reflexive ");
                if (IsSymmetric)
                    b.Append("symmetric ");
                if (IsAntiSymmetric)
                    b.Append("anti-symmetric ");
                b.AppendLine();
            }

            if (ObjectLowerBound > 0 || ObjectUpperBound < Unbounded)
            {
                if (ObjectLowerBound == ObjectUpperBound)
                    b.Append($"Subjects {PluralForm.Untokenize()} {ObjectLowerBound} objects");
                else if (ObjectUpperBound == Verb.Unbounded)
                    b.Append($"Subjects {PluralForm.Untokenize()} at least {ObjectLowerBound} objects");
                else if (ObjectLowerBound == 0)
                    b.Append($"Subjects {PluralForm.Untokenize()} at most {ObjectUpperBound} objects");
                else 
                    b.Append($"Subjects {PluralForm.Untokenize()} {ObjectLowerBound}-{ObjectUpperBound} objects");
                b.AppendLine();
            }

            if (SubjectLowerBound > 0 || SubjectUpperBound < Unbounded)
            {
                if (SubjectLowerBound == SubjectUpperBound)
                    b.Append($"Objects are {PassiveParticiple.Untokenize()} by {SubjectLowerBound} subjects");
                else if (SubjectUpperBound == Unbounded)
                    b.Append($"Objects are {PassiveParticiple.Untokenize()} by at least {SubjectLowerBound} subjects");
                else if (SubjectLowerBound == 0)
                    b.Append($"Objects are {PassiveParticiple.Untokenize()} by at most {SubjectUpperBound} subjects");
                else 
                    b.Append($"Objects are {PassiveParticiple.Untokenize()} by {SubjectLowerBound}-{SubjectUpperBound} subjects");
                b.AppendLine();
            }

            return b.ToString();
        }
    }

    protected override string DictionaryStylePartOfSpeech => "v.";

    /// <summary>
    /// Verbs that are implied by this verb
    /// </summary>
    public List<Verb> Generalizations = new List<Verb>();

    /// <summary>
    /// Verbs that are mutually exclusive with this one: A this B implies not A exclusion B
    /// </summary>
    public List<Verb> MutualExclusions = new List<Verb>();

    public List<Verb> Subspecies = new List<Verb>();
    // ReSharper disable once IdentifierTypo
    public List<Verb> Superspecies = new List<Verb>();

    /// <summary>
    /// The value for an upper bound that means there is no upper bound
    /// This can be any large value but must not be short.MaxValue, or there will be overflow errors.
    /// </summary>
    public const int Unbounded = 10000;

    /// <summary>
    /// The maximum number of elements in the Object domain, a given member of the Subject domain can be related to.
    /// </summary>
    public int ObjectUpperBound = Unbounded;
    /// <summary>
    /// The minimum number of elements in the Object domain, a given member of the Subject domain can be related to.
    /// </summary>
    public int ObjectLowerBound;

    /// <summary>
    /// The maximum number of elements in the Subject domain, a given member of the Object domain can be related to.
    /// </summary>
    public int SubjectUpperBound = Unbounded;
    /// <summary>
    /// The minimum number of elements in the Subject domain, a given member of the Object domain can be related to.
    /// </summary>
    public int SubjectLowerBound;

///// <summary>
///// There is an object for every possible subject.
///// </summary>
//public bool IsTotal
//    {
//        get => ObjectLowerBound == 1;
//        set => ObjectLowerBound = value?Math.Max(1, ObjectLowerBound):ObjectLowerBound;
//    }

    public bool IsReflexive;

    public bool AncestorIsReflexive => IsReflexive || Superspecies.Any(sup => sup.AncestorIsReflexive);

    public bool IsAntiReflexive;

    public bool AncestorIsAntiReflexive => IsAntiReflexive || Superspecies.Any(sup => sup.AncestorIsAntiReflexive);

    public bool IsSymmetric;

    public bool IsAntiSymmetric;

    /// <summary>
    /// The initial probability of the relation.
    /// </summary>
    public float Density = 0.5f;

    public override bool IsNamed(string[] tokens) => tokens.SameAs(SingularForm) || tokens.SameAs(PluralForm);

    // ReSharper disable InconsistentNaming
    private string[] _baseForm;
    private string[] _gerundForm;

    // ReSharper restore InconsistentNaming

    public string[] BaseForm
    {
        get => _baseForm;
        set
        {
            _baseForm = value;
            Ontology.VerbTrie.Store(value, this);
            EnsureGerundForm();
            EnsurePassiveParticiple();
            EnsurePluralForm();
            EnsureSingularForm();
        }
    }

    public string[] PassiveParticiple { get; private set;  }

    public string[] GerundForm
    {
        get => _gerundForm;
        set
        {
            _gerundForm = value;
            Ontology.VerbTrie.Store(value, this);
            EnsureBaseForm();
            EnsurePluralForm();
            EnsureSingularForm();
        }
    }

    public override string[] StandardName => BaseForm;

    // ReSharper disable InconsistentNaming
    private string[] _singular, _plural;

    // ReSharper restore InconsistentNaming

    /// <summary>
    /// Singular form of the verb
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
            Ontology.EnsureUndefinedOrDefinedAsType(value, GetType());
            if (_singular != null) Ontology.VerbTrie.Store(_singular, null);
            _singular = value;
            Ontology.VerbTrie.Store(_singular, this);
            EnsurePluralForm();
            EnsureGerundForm();
        }
    }

    /// <summary>
    /// Add likely spellings of the gerund of this verb.
    /// They are stored as if they are plural inflections.
    /// </summary>
    private void EnsureGerundForm()
    {
        if (_gerundForm != null)
            return;
        EnsureBaseForm();
        foreach (var form in Inflection.GerundsOfVerb(_baseForm))
        {
            if (_gerundForm == null)
                _gerundForm = form;
            Ontology.VerbTrie.Store(form, this, true);
        }
    }

    /// <summary>
    /// Add likely spellings of the gerund of this verb.
    /// They are stored as if they are plural inflections.
    /// </summary>
    private void EnsurePassiveParticiple()
    {
        if (PassiveParticiple != null)
            return;
        EnsureBaseForm();
        PassiveParticiple = Inflection.PassiveParticiple(BaseForm);
        Ontology.VerbTrie.Store(PassiveParticiple, this, true);
    }

    private void EnsureBaseForm()
    {
        if (_baseForm != null)
            return;
        if (_gerundForm != null)
            _baseForm = Inflection.BaseFormOfGerund(_gerundForm);
        Debug.Assert(_plural != null || _singular != null || _baseForm != null);
        EnsurePluralForm();
        EnsureSingularForm();
        if (_baseForm != null)
            _baseForm = Inflection.ReplaceCopula(_plural, "be");
        EnsureGerundForm();
    }

    /// <summary>
    /// Make sure the noun has a singular verb.
    /// </summary>
    private void EnsureSingularForm()
    {
        if (_singular == null)
            SingularForm = Inflection.SingularOfVerb(_plural);
    }

    /// <summary>
    /// Plural form of the verb
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
            Ontology.EnsureUndefinedOrDefinedAsType(value, GetType());
            if (_plural != null) Ontology.VerbTrie.Store(_plural, null);
            _plural = value;
            Ontology.VerbTrie.Store(_plural, this, true);
            EnsureSingularForm();
        }
    }

    public CommonNoun SubjectKind { get; set; }
    public MonadicConceptLiteral[] SubjectModifiers { get; set; }
    public CommonNoun ObjectKind { get; set; }
    public MonadicConceptLiteral[] ObjectModifiers { get; set; }

    private void EnsurePluralForm()
    {
        if (_plural != null)
            return;
        if (_baseForm != null)
            PluralForm = Inflection.ReplaceCopula(_baseForm, "are"); 
        else
            PluralForm = Inflection.PluralOfVerb(_singular);
    }
}

public enum VerbConjugation
{
    ThirdPerson,
    BaseForm,
    Gerund,
    PassiveParticiple
};