﻿using System;
using static Parser;

/// <summary>
/// Scans a contiguous sequence of input tokens
/// </summary>
public class Segment
{
    /// <summary>
    /// Scan forward from current position to the occurence of token.
    /// Text will not include token, and terminating token will not be skipped.
    /// </summary>
    /// <param name="token">Token marking the end of the segment</param>
    /// <returns>True on success</returns>
    public virtual bool ScanTo(string token)
    {
        var beginning = State;
        while (!EndOfInput)
            if (Syntax.ListConjunction(CurrentToken))
                return false;
            else if (token == CurrentToken)
            {
                SetText(beginning);
                return true;
            }
            else
                SkipToken();

        ResetTo(beginning);
        return false;
    }

    /// <summary>
    /// Scan forward from current position to the first token satisfying endPredicate
    /// Text will not include terminating token, and terminating token will not be skipped.
    /// </summary>
    /// <param name="endPredicate">Test recognize tokens that come after segment</param>
    /// <returns>True on success</returns>
    public virtual bool ScanTo(Func<string, bool> endPredicate)
    {
        var beginning = State;
        while (!EndOfInput)
            if (Match(endPredicate))
            {
                Backup();
                SetText(beginning);
                return true;
            }
        else if (Syntax.ListConjunction(CurrentToken))
            return false;
        else
                SkipToken();

        ResetTo(beginning);
        return false;
    }

    /// <summary>
    /// Set segment to all remaining tokens.
    /// Fails when remaining tokens includes a conjunction, unless failOnConjunction is set to false.
    /// </summary>
    /// <param name="failOnConjunction">True if segment should not include a conjunction ("and" or "or").</param>
    /// <returns>True on success.</returns>
    public virtual bool ScanToEnd(bool failOnConjunction = true)
    {
        if (EndOfInput)
            return false;
        var beginning = State;
        if (failOnConjunction)
            // Have to check to make sure there's no embedded conjunction
            while (!EndOfInput)
            {
                if (Syntax.ListConjunction(CurrentToken))
                    return false;
                SkipToken();
            }
        else
            SkipToEnd();

        SetText(beginning);
        return true;
    }

    private void SetText(ParserState from)
    {
        SetText(from, State);
    }

    private void SetText(ParserState from, ParserState to)
    {
        start = from.CurrentTokenIndex;
        end = to.CurrentTokenIndex;
    }

    private int start;
    private int end;

    /// <summary>
    /// Number of tokens in the text scanned by this segment.
    /// </summary>
    public int Length => end - start;

    /// <summary>
    /// The position'th token of this segment, counting from zero.
    /// </summary>
    public string this[int position] => Input[start + position];

    /// <summary>
    /// Succeeds if specified tokens appear next, in order, in the remaining input.
    /// Skips over tokens.
    /// </summary>
    public bool MatchSegment(string[] tokens)
    {
        for (int i = 0; i < Length; i++)
            if (this[i] != tokens[i])
                return false;
        return true;
    }

    /// <summary>
    /// The token string matched by this segment.
    /// </summary>
    public string[] Text
    {
        get
        {
            var length = end - start;
            var result = new string[length];
            Input.CopyTo(start, result, 0, length);
            return result;
        }
    }

    /// <summary>
    /// Name of this segment, for use in printing doc strings
    /// </summary>
    public string Name;
}