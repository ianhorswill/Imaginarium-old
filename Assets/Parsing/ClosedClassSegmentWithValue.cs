using System;
using System.Collections.Generic;
using System.Linq;
using static Parser;

public class ClosedClassSegmentWithValue<T> : Segment
{
    /// <summary>
    /// The token that was used as a determiner;
    /// </summary>
    public string[] Match;

    public T Value;

    public readonly KeyValuePair<string[], T>[] PossibleMatches;
    public readonly string[] PossibleBeginnings;

    public ClosedClassSegmentWithValue(params KeyValuePair<object, T>[] possibleMatches)
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
            if (Match(candidate.Key))
            {
                Match = candidate.Key;
                Value = candidate.Value;
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
            if (Match(candidate.Key))
            {
                Match = candidate.Key;
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
        Match = null;
        foreach (var candidate in PossibleMatches)
        {
            if (Match(candidate.Key))
            {
                Match = candidate.Key;
                Value = candidate.Value;
                break;
            }
            ResetTo(old);
        }

        return EndOfInput;
    }
}