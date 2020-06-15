﻿#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClosedClassSegmentWithValue.cs" company="Ian Horswill">
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

public class ClosedClassSegmentWithValue<T> : ClosedClassSegment
{
    public T Value;

    public readonly KeyValuePair<string[], T>[] PossibleMatches;

    public ClosedClassSegmentWithValue(Parser parser, params KeyValuePair<object, T>[] possibleMatches) : base(parser)
    {
        PossibleMatches = possibleMatches.Select(m =>
        {
            switch (m.Key)
            {
                case string s:
                    return new KeyValuePair<string[], T>(new[] {s}, m.Value);

                case string[] array:
                    return new KeyValuePair<string[], T>(array, m.Value);

                default: throw new ArgumentException($"Invalid match argument {m}");
            }
        }).ToArray();
        PossibleBeginnings = PossibleMatches.Select(p => p.Key[0]).Distinct().ToArray();
        IsPossibleStart = token => PossibleBeginnings.Contains(token);
    }

    public override bool ScanTo(Func<string, bool> endPredicate)
    {
        if (!IsPossibleStart(CurrentToken))
            return false;
        var old = State;
        MatchedText = null;
        foreach (var candidate in PossibleMatches)
        {
            if (Match(candidate.Key))
            {
                MatchedText = candidate.Key;
                Value = candidate.Value;
                break;
            }
            ResetTo(old);
        }

        return !EndOfInput && endPredicate(CurrentToken);
    }

    public override bool ScanTo(string token)
    {
        if (EndOfInput || !IsPossibleStart(CurrentToken))
            return false;
        var old = State;
        MatchedText = null;
        foreach (var candidate in PossibleMatches)
        {
            if (Match(candidate.Key))
            {
                MatchedText = candidate.Key;
                Value = candidate.Value;
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
        MatchedText = null;
        foreach (var candidate in PossibleMatches)
        {
            if (Match(candidate.Key))
            {
                MatchedText = candidate.Key;
                Value = candidate.Value;
                break;
            }
            ResetTo(old);
        }

        return EndOfInput;
    }

    public override IEnumerable<string> Keywords
    {
        get
        {
            foreach (var a in PossibleMatches)
            foreach (var s in a.Key)
                yield return s;
        }
    }
}