#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Segment.cs" company="Ian Horswill">
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
        if (EndOfInput)
            return false;
        ParseModifiers();
        if (EndOfInput || !ValidBeginning(CurrentToken))
            return false;
        var beginning = State;
        while (!EndOfInput)
            if (Syntax.ListConjunction(CurrentToken))
                return false;
            else if (token == CurrentToken)
            {
                SetText(beginning);
                if (start < end) // Must have consumed at least one token
                    return true;
                else
                    goto fail;
            }
            else
                SkipToken();

        fail:
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
        if (EndOfInput)
            return false;
        ParseModifiers();
        if (EndOfInput || !ValidBeginning(CurrentToken))
            return false;
        var beginning = State;
        while (!EndOfInput)
            if (Match(endPredicate))
            {
                Backup();
                SetText(beginning);
                if (start == end)
                    // Empty segment
                    goto giveUp;
                return true;
            }
            else if (Syntax.ListConjunction(CurrentToken))
            {
                ResetTo(beginning);
                return false;
            }
            else
                SkipToken();

        giveUp:
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
        ParseModifiers();
        if (EndOfInput)
            return false;
        if (!ValidBeginning(CurrentToken))
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
        if (start == end)
        {
            ResetTo(beginning);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Tests whether the first token of a segment is a valid start to the segment.
    /// </summary>
    /// <param name="firstToken">first token of the segment</param>
    /// <returns>True if this is a valid start to the sentence.</returns>
    public virtual bool ValidBeginning(string firstToken) => true;

    /// <summary>
    /// Swallow any special preliminary words like determiners
    /// </summary>
    public virtual void ParseModifiers()
    { }

    protected void SetText(ScannerState from)
    {
        SetText(from, State);
    }

    private void SetText(ScannerState from, ScannerState to)
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
    /// The position-th token of this segment, counting from zero.
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
    public virtual string[] Text
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