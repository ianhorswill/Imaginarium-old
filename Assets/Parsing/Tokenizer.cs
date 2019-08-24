using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Converts between text strings and their tokenized forms.
/// </summary>
public static class Tokenizer
{
    private static string input;
    private static int currentCharIndex;

    private static char CurrentChar => input[currentCharIndex];
    private static bool EndOfInput => currentCharIndex == input.Length;

    /// <summary>
    /// Convert text to a sequence of tokens.
    /// </summary>
    public static IEnumerable<string> Tokenize(string text)
    {
        input = text;
        currentCharIndex = 0;
        SkipWhiteSpace();
        while (!EndOfInput)
        {
            var c = CurrentChar;
            if (Char.IsLetter(c))
                yield return GetTokenWhile(Char.IsLetter);
            else if (Char.IsDigit(c))
                yield return GetTokenWhile(Char.IsDigit);
            else if (Char.IsPunctuation(c))
            {
                yield return new string(c, 1);
                SkipChar();
            }
            else throw new GrammaticalError($"Unknown character '{c}'");
            SkipWhiteSpace();
        }
    }

    private static readonly StringBuilder TokenBuffer = new StringBuilder();
    private static string GetTokenWhile(Func<char, bool> tokenCriterion)
    {
        TokenBuffer.Length = 0;
        while (!EndOfInput && tokenCriterion(CurrentChar))
        {
            TokenBuffer.Append(CurrentChar);
            SkipChar();
        }

        return TokenBuffer.ToString();
    }

    private static void SkipWhiteSpace()
    {
        SkipWhile(Char.IsWhiteSpace);
    }

    private static void SkipWhile(Func<char, bool> skipCriterion)
    {
        while (!EndOfInput && skipCriterion(CurrentChar))
            SkipChar();
    }

    private static void SkipChar()
    {
        currentCharIndex++;
    }

    /// <summary>
    /// Convert a sequence of tokens into a single text string, adding spaces where appropriate.
    /// </summary>
    public static string Untokenize(IEnumerable<string> tokens)
    {
        var b = new StringBuilder();
        var firstOne = true;
        var lastToken = "";
        foreach (var t in tokens)
        {
            if (!PunctuationToken(t) && !t.StartsWith("<"))
            {
                if (firstOne)
                    firstOne = false;
                else if (lastToken != "-" && !lastToken.StartsWith("<"))
                    b.Append(' ');
            }

            b.Append(t);
            if (!t.StartsWith("<"))
                lastToken = t;
        }

        return b.ToString();
    }

    private static bool PunctuationToken(string s) => s.Length == 1 && char.IsPunctuation(s[0]);
}
