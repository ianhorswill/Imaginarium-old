#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClosedClassSegment.cs" company="Ian Horswill">
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
using System.Linq;
using static Parser;

public class ClosedClassSegment : Segment
{
    /// <summary>
    /// The token that was used as a determiner;
    /// </summary>
    public string[] Match;

    public readonly string[][] PossibleMatches;
    public readonly string[] PossibleBeginnings;

    public ClosedClassSegment(params object[] possibleMatches)
    {
        PossibleMatches = possibleMatches.Select(m =>
        {
            switch (m)
            {
                case string s:
                    return new[] {s};

                case string[] array:
                    return array;

                default: throw new ArgumentException($"Invalid match argument {m}");
            }
        }).ToArray();
        PossibleBeginnings = PossibleMatches.Select(a => a[0]).Distinct().ToArray();
        IsPossibleStart = token => PossibleBeginnings.Contains(token);
    }

    /// <summary>
    /// Tests if the token is one of the known quantifiers
    /// </summary>
    public readonly Func<string, bool> IsPossibleStart;

    public override bool ScanTo(Func<string, bool> endPredicate)
    {
        if (!IsPossibleStart(CurrentToken))
            return false;
        var old = State;
        Match = null;
        foreach (var candidate in PossibleMatches)
        {
            if (Match(candidate))
            {
                Match = candidate;
                break;
            }
            ResetTo(old);
        }

        return !EndOfInput && endPredicate(CurrentToken);
    }

    public override bool ScanTo(string token)
    {
        if (!IsPossibleStart(CurrentToken))
            return false;
        var old = State;
        Match = null;
        foreach (var candidate in PossibleMatches)
        {
            if (Match(candidate))
            {
                Match = candidate;
                break;
            }
            ResetTo(old);
        }

        return !EndOfInput && CurrentToken == token;
    }

    public override bool ScanToEnd(bool failOnConjunction = true)
    {
        if (!IsPossibleStart(CurrentToken))
            return false;
        var old = State;
        Match = null;
        foreach (var candidate in PossibleMatches)
        {
            if (Match(candidate))
            {
                Match = candidate;
                break;
            }
            ResetTo(old);
        }

        return EndOfInput;
    }
}