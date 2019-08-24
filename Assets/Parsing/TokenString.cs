using System.Diagnostics;

/// <summary>
/// A read-only sequence of tokens that can be matched in a case-independent manner.
/// </summary>
[DebuggerDisplay("TokenString {" + nameof(Text) + "}")]
public struct TokenString
{
    /// <summary>
    /// Tokens in the sequence
    /// </summary>
    public readonly string[] Tokens;

    // ReSharper disable once IdentifierTypo
    private readonly string[] downcased;

    public TokenString(string[] tokens)
    {
        Tokens = tokens;
        downcased = new string[Tokens.Length];
        for (var i = 0; i < Tokens.Length; i++)
            downcased[i] = Tokens[i].ToLowerInvariant();
    }

    public override int GetHashCode()
    {
        var hash = 0;
        foreach (var t in downcased)
            hash ^= t.GetHashCode();
        return hash;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is TokenString))
            return false;
        var ot = (TokenString) obj;
        if (ot.downcased.Length != downcased.Length)
            return false;
        for (var i = 0; i < downcased.Length; i++)
            if (downcased[i] != ot.downcased[i])
                return false;
        return true;
    }

    public string Text => downcased.ToString();

    public static implicit operator TokenString(string[] tokens)
    {
        return new TokenString(tokens);
    }

    public static implicit operator string[](TokenString t)
    {
        return t.Tokens;
    }
}
