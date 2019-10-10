#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TokenString.cs" company="Ian Horswill">
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

    public TokenString(params string[] tokens)
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

    public static implicit operator TokenString(string word)
    {
        return new TokenString(word);
    }

    public static implicit operator string[](TokenString t)
    {
        return t.Tokens;
    }
}
