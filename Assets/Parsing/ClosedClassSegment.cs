using System;
using System.Collections.Generic;

/// <summary>
/// Base class for segments that can only be filled by fixed words and phrases
/// </summary>
public abstract class ClosedClassSegment : Segment
{
    /// <summary>
    /// The token that was used as a determiner;
    /// </summary>
    public string[] MatchedText;
    public string[] PossibleBeginnings;

    /// <summary>
    /// Tests if the token is one of the known quantifiers
    /// </summary>
    public Func<string, bool> IsPossibleStart;

    public abstract IEnumerable<string> Keywords { get; }

    protected ClosedClassSegment(Parser parser) : base(parser)
    { }
}

