using System;

/// <summary>
/// Base class for exceptions believed to be caused by a user's mistake.
/// </summary>
public class UserException : Exception
{
    /// <summary>
    /// Rich text version of the error message
    /// </summary>
    public readonly string RichText;
    protected UserException(string message, string richText) : base(message)
    {
        RichText = richText;
    }
}