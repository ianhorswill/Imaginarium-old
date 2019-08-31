using System.Collections.Generic;

/// <summary>
/// Represents a verb, i.e. a binary relation
/// </summary>
public class Verb : Concept
{
    /// <summary>
    /// There is at most one object for each possible subject
    /// </summary>
    public bool IsFunction;
    /// <summary>
    /// There is an object for every possible subject.
    /// </summary>
    public bool IsTotal;

    public bool IsReflexive;

    public bool IsAntiReflexive;

    public bool IsSymmetric;

    public bool IsAntiSymmetric;

    /// <summary>
    /// The initial probability of the relation.
    /// </summary>
    public float Density = 0.5f;

    public static readonly TokenTrie<Verb> Trie = new TokenTrie<Verb>();

    public static IEnumerable<Verb> AllVerbs => Trie.Contents;

    public override bool IsNamed(string[] tokens) => tokens.SameAs(SingularForm) || tokens.SameAs(PluralForm);

    public override string[] StandardName => SingularForm;

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
            if (_singular != null) Trie.Store(_singular, null);
            _singular = value;
            Trie.Store(_singular, this);
            EnsurePluralForm();
            CreateGerundForms();
        }
    }

    /// <summary>
    /// Add likely spellings of the gerund of this verb.
    /// They are stored as if they are plural inflections.
    /// </summary>
    private void CreateGerundForms()
    {
        foreach (var form in Inflection.GerundOfVerb(_plural))
            Trie.Store(form, this, true);
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
            if (_plural != null) Trie.Store(_plural, null);
            _plural = value;
            Trie.Store(_plural, this, true);
            EnsureSingularForm();
        }
    }

    public CommonNoun SubjectKind { get; set; }
    public CommonNoun ObjectKind { get; set; }

    private void EnsurePluralForm()
    {
        if (_plural == null)
            PluralForm = Inflection.PluralOfVerb(_singular);
    }

    public static Verb Find(params string[] tokens)
    {
        int index = 0;
        var v = Trie.Lookup(tokens, ref index);
        if (index != tokens.Length)
            return null;
        return v;
    }
}
