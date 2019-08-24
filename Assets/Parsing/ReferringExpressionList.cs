using System;
using System.Collections.Generic;
using System.Linq;
using static Parser;

/// <summary>
/// A Segment that refers to a list of some Referents
/// </summary>
/// <typeparam name="TE">Type of the constituent ReferringExpressions</typeparam>
/// <typeparam name="TR">Type of the Referents of the constituent expressions</typeparam>
public class ReferringExpressionList<TE, TR> : Segment
    where TE: ReferringExpression<TR>, new()
    where TR : Referent
{
    /// <summary>
    /// Validates constituent expressions during parsing.  Must return true for each constituent
    /// or the parse fails.
    /// </summary>
    public Func<TE, bool> SanityCheck = x => true;

    /// <summary>
    /// Internal cached list of referents.
    /// </summary>
    private readonly List<TR> _concepts = new List<TR>();

    /// <summary>
    /// List of constituent expressions
    /// </summary>
    readonly List<TE> expressions = new List<TE>();

    /// <summary>
    /// True if the constituents are joined by "and", otherwise they're joined by "or"
    /// </summary>
    public bool IsAnd;

    /// <summary>
    /// The list of concepts referred to by the constituent expressions
    /// </summary>
    public List<TR> Concepts
    {
        get
        {
            // This assumes we will never have an expression whose referent is the empty list
            if (_concepts.Count == 0)
                _concepts.AddRange(expressions.Select(e => e.Concept));
            return _concepts;
        }
    }

    /// <summary>
    /// Reset internal state so we can reparse.
    /// </summary>
    public virtual void Reset()
    {
        _concepts.Clear();
        expressions.Clear();
    }

    /// <summary>
    /// Makes a function that tests whether an argument token terminates an individual list item.
    /// Function returns true when argument token satisfies test, is a list conjunction, or a comma.
    /// </summary>
    /// <param name="overallTerminator">User-supplied test for terminating a list item.</param>
    /// <returns>True if the token marks the end of a list item</returns>
    Func<string, bool> ListItemTerminator(Func<string, bool> overallTerminator)
    {
        return token => overallTerminator(token) || token == "," || Syntax.ListConjunction(token);
    }

    /// <summary>
    /// Scan a list of constituent expressions until a token satisfying endPredicate is found.
    /// </summary>
    /// <param name="endPredicate">Test for end of the list</param>
    /// <returns>True on success</returns>
    public override bool ScanTo(Func<string, bool> endPredicate)
    {
        var lastOne = false;
        var done = false;
        while (!endPredicate(CurrentToken))
        {
            var item = new TE();
            if (!item.ScanTo(ListItemTerminator(endPredicate)))
                return false;
            if (!SanityCheck(item))
                return false;
            expressions.Add(item);
            if (lastOne)
                done = true;
            else if (CurrentToken == "and")
            {
                lastOne = true;
                IsAnd = true;
                SkipToken();
            } else if (CurrentToken == "or")
            {
                lastOne = true;
                IsAnd = false;
                SkipToken();
            }
            else
            {
                if (!endPredicate(CurrentToken))
                    SkipToken();
                if (CurrentToken == "and")
                {
                    lastOne = true;
                    IsAnd = true;
                    SkipToken();
                } else if (CurrentToken == "or")
                {
                    lastOne = true;
                    IsAnd = false;
                    SkipToken();
                }
            }
        }

        if (!done)
            return false;
        return true;
    }

    /// <summary>
    /// Sequence of items until end of input.  Fails if a non-item is found before end of input.
    /// </summary>
    /// <param name="ignore">Not used.  This is the failOnConjunction argument from the Segment class, and we would never want to fail on conjunction for a list.</param>
    /// <returns>True on success, false if scanner encountered something that wasn't a valid constituent</returns>
    public override bool ScanToEnd(bool ignore = true)
    {
        var lastOne = false;
        var done = false;
        while (!EndOfInput)
        {
            var item = new TE();
            var scanTo = lastOne?item.ScanToEnd(false):item.ScanTo(ListItemTerminator(t => false));
            if (!scanTo)
                return false;
            if (!SanityCheck(item))
                return false;
            expressions.Add(item);
            if (lastOne)
                done = true;
            else if (CurrentToken == "and")
            {
                lastOne = true;
                IsAnd = true;
                SkipToken();
            } else if (CurrentToken == "or")
            {
                lastOne = true;
                IsAnd = false;
                SkipToken();
            }
            else
            {
                if (EndOfInput)
                    break;
                if (CurrentToken == "and")
                {
                    lastOne = true;
                    IsAnd = true;
                    SkipToken();
                } else if (CurrentToken == "or")
                {
                    lastOne = true;
                    IsAnd = false;
                    SkipToken();
                } else 
                    SkipToken();
            }
        }

        if (!done)
            return false;
        return true;
    }
}