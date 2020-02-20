#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuantifyingDeterminer.cs" company="Ian Horswill">
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
using System.Linq;
using static Parser;

public class QuantifyingDeterminer : ClosedClassSegment
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
        "some",
        "other"
    };

    public override IEnumerable<string> Keywords
    {
        get
        {
            foreach (var s in SingularQuantifiers)
                yield return s;
            foreach (var s in PluralQuantifiers)
                yield return s;
        }
    }

    private static readonly string[] InvalidQuantifiers =
    {
        "a",
    };

    public bool IsPlural => PluralQuantifiers.Contains(Quantifier);
    public bool IsOther;
    public bool IsInvalid => InvalidQuantifiers.Contains(Quantifier);

    public override bool ScanTo(Func<string, bool> endPredicate)
    {
        Quantifier = CurrentToken;
        if (!Match(IsQuantifier))
            return false;

        IsOther = Quantifier == "other";

        if (!IsOther && !EndOfInput && CurrentToken == "other")
        {
            IsOther = true;
            SkipToken();
        }

        return !EndOfInput && endPredicate(CurrentToken);
    }

    public override bool ScanTo(string token)
    {
        Quantifier = CurrentToken;
        if (!Match(IsQuantifier))
            return false;

        IsOther = Quantifier == "other";

        if (!IsOther && !EndOfInput && CurrentToken == "other")
        {
            IsOther = true;
            SkipToken();
        }

        return !EndOfInput && CurrentToken == token;
    }

    public override bool ScanToEnd(bool failOnConjunction = true)
    {
        Quantifier = CurrentToken;
        return Match(IsQuantifier) && EndOfInput;
    }
}
