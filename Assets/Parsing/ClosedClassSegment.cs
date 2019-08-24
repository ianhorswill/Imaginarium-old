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