using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// A monadic concept that can be realized in English as the head of an NP.
/// </summary>
[DebuggerDisplay("{" + nameof(Text) + "}")]
public class Noun : MonadicConcept
{
    static Noun()
    {
        Ontology.AllReferentTables.Add(AllNouns);
    }

    public static Dictionary<TokenString, Noun> AllNouns = new Dictionary<TokenString, Noun>();

    /// <summary>
    /// Returns the noun named by the specified token string, or null if there is none.
    /// </summary>
    public static Noun Find(params string[] tokens) => AllNouns.LookupOrDefault(tokens);

    /// <inheritdoc />
    public override bool IsNamed(string[] tokens)
    {
        return SingularForm.SameAs(tokens) || PluralForm.SameAs(tokens);
    }

    private string[] _singular, _plural;

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
 }