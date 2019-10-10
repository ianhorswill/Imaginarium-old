#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tokenizer.cs" company="Ian Horswill">
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
