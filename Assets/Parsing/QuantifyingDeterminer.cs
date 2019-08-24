using System;
using System.Linq;
using static Parser;

public class QuantifyingDeterminer : Segment
{
    /// <summary>
    /// The token that was used as a determiner;
    /// </summary>
    public string Quantifier;

    /// <summary>
    /// Tests if the token is one of the known quantifiers
    /// </summary>
    public readonly Func<string, bool> IsQuantifier = token => 
        SingularQuantifiers.Contains(token) || PluralQuantifiers.Contains(token)
                                            || InvalidQuantifiers.Contains(token);

    private static readonly string[] SingularQuantifiers =
    {
        "one",
    };

    private static readonly string[] PluralQuantifiers =
    {
        "many",
        "other"
    };

    private static readonly string[] InvalidQuantifiers =
    {
        "a",
    };

    public bool IsPlural => PluralQuantifiers.Contains(Quantifier);
    public bool IsInvalid => InvalidQuantifiers.Contains(Quantifier);

    public override bool ScanTo(Func<string, bool> endPredicate)
    {
        Quantifier = CurrentToken;
        return Match(IsQuantifier) && !EndOfInput && endPredicate(CurrentToken);
    }

    public override bool ScanTo(string token)
    {
        Quantifier = CurrentToken;
        return Match(IsQuantifier) && !EndOfInput && CurrentToken == token;
    }

    public override bool ScanToEnd(bool failOnConjunction = true)
    {
        Quantifier = CurrentToken;
        return Match(IsQuantifier) && EndOfInput;
    }
}
