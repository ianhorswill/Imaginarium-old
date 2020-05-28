#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Inflection.cs" company="Ian Horswill">
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

using System.Collections.Generic;
using System.Data;
using System.Linq;
using File = System.IO.File;

/// <summary>
/// Implements a best effort to convert between English plural and singular noun inflections
/// </summary>
public static class Inflection
{
    static Inflection()
    {
        Inflections = InflectionProcess.FromFile(ConfigurationFiles.PathTo("Inflections", "Regular nouns"));

        foreach (var entry in File.ReadAllLines(ConfigurationFiles.PathTo("Inflections", "Irregular nouns")))
        {
            var split = entry.Split('\t');
            var singular = split[0];
            var plural = split[1];
            IrregularPlurals[singular] = plural;
            IrregularSingulars[plural] = singular;
        }

        irregularVerbs = new Spreadsheet(ConfigurationFiles.PathTo(
                "Inflections", "Irregular verbs", ".csv"),
                "Base form");
    }

    private static readonly Spreadsheet irregularVerbs;
    public static string[] PluralOfNoun(string[] singular)
    {
        var plural = new string[singular.Length];
        singular.CopyTo(plural,0);
        var last = singular.Length - 1;
        plural[last] = PluralOfNoun(singular[last]);
        return plural;
    }

    public static string PluralOfNoun(string singular)
    {
        if (IrregularPlurals.TryGetValue(singular, out string plural))
            return plural;
        foreach (var i in Inflections)
            if (i.MatchSingularForPlural(singular))
                return i.InflectSingularForPlural(singular);
        throw new GrammaticalError($"'{singular}' appears to be a singular noun, but I can't find a plural inflection for it",
            $"In this context, the term '<i>{singular}</i>' appears to be a singular noun, but I can't find a plural inflection for it");
    }

    public static string SingularOfNoun(string plural)
    {
        if (IrregularSingulars.TryGetValue(plural, out string singular))
            return singular;
        foreach (var i in Inflections)
            if (i.MatchPluralForSingular(plural))
                return i.InflectPluralForSingular(plural);
        throw new GrammaticalError($"'{plural}' appears to be a plural noun, but I can't find a singular inflection for it",
            $"In this context, the term '<i>{plural}</i>' appears to be a plural noun, but I can't find a singular inflection for it");
    }

    public static string[] SingularOfNoun(string[] plural)
    {
        var singular = new string[plural.Length];
        plural.CopyTo(singular,0);
        var last = plural.Length - 1;
        singular[last] = SingularOfNoun(plural[last]);
        return singular;
    }

    public static bool NounAppearsPlural(string plural)
    {
        if (IrregularSingulars.TryGetValue(plural, out string singular))
            return true;
        foreach (var i in Inflections)
            if (i.MatchPluralForSingular(plural))
                return true;
        return false;
    }

    public static bool NounAppearsPlural(string[] plural)
    {
        return NounAppearsPlural(plural[plural.Length-1]);
    }

    public static string[] SingularOfVerb(string[] plural)
    {
        if (ContainsCopula(plural))
            return ReplaceCopula(plural, "is");        return PluralOfNoun(plural);
    }

    public static string[] PluralOfVerb(string[] singular)
    {
        if (ContainsCopula(singular))
            return ReplaceCopula(singular, "are");
        return SingularOfNoun(singular);
    }

    public static bool IsGerund(string[] verbal) =>
        ContainsCopula(verbal) || (verbal.Length == 1 && verbal[0].EndsWith("ing"));

    public static IEnumerable<string[]> GerundsOfVerb(string[] plural)
    {
        if (ContainsCopula(plural))
            yield return ReplaceCopula(plural, "being");
        else if (plural.Length == 1)
        {
            var s = plural[0];
            if (EndsWithVowel(s))
                yield return new[] { WithoutFinalCharacter(s) + "ing" };
            else
                yield return new[] {s + "ing"};

            if (EndingConsonant(s, out var terminalConsonant))
            {
                yield return new [] { s + terminalConsonant.ToString() + "ing" };
            }
            else
            {
                yield return new[] {s.Substring(0, s.Length - 1) + "ing"};
            }
        }
    }

    public static string[] BaseFormOfGerund(string[] gerund)
    {
        if (gerund.Contains("being"))
        {
            return gerund.Replace("being", "be").ToArray();
        }
        if (gerund.Length == 1)
        {
            var s = gerund[0];
            // Cut trailing -ing
            if (s.EndsWith("ing"))
                s = s.Substring(0, s.Length - 3);
            var len = s.Length;
            // Removed doubled consonant
            if (len > 2 && s[len - 1] == s[len - 2])
                s = s.Substring(0, len - 1);
            return new [] { s };
        }

        throw new SyntaxErrorException($"Can't determine the stem verb of gerund {gerund.Untokenize()}");
    }

    private static readonly char[] Vowels = {'a', 'e', 'i', 'o', 'u'};
    private static bool IsVowel(char c) => Vowels.Contains(c);
    private static bool IsConsonant(char c) => !IsVowel(c);
    private static bool EndsWithVowel(string s) => IsVowel(FinalCharacter(s));
    private static bool EndsWithConsonant(string s) => IsConsonant(FinalCharacter(s));

    private static bool EndingConsonant(string s, out char c)
    {
        System.Diagnostics.Debug.Assert(s.Length > 0);
        c = FinalCharacter(s);
        return IsVowel(c);
    }

    private static char FinalCharacter(string s)
    {
        return s[s.Length - 1];
    }

    private static string WithoutFinalCharacter(string s) => s.Substring(0, s.Length - 1);

    // ReSharper disable once IdentifierTypo
    private static readonly string[] CopularForms = {"is", "are", "being", "be" };
    private static bool ContainsCopula(string[] tokens) => tokens.Any(word => CopularForms.Contains(word));

    public static string[] ReplaceCopula(string[] tokens, string replacement) => tokens.Select(word => CopularForms.Contains(word) ? replacement : word).ToArray();

    private static IEnumerable<T> Replace<T>(this IEnumerable<T> seq, T from, T to) =>
        seq.Select(e => e.Equals(from) ? to : e);

    private static readonly Dictionary<string, string> IrregularPlurals = new Dictionary<string, string>();

    private static readonly  Dictionary<string, string> IrregularSingulars = new Dictionary<string, string>();

    private static readonly InflectionProcess[] Inflections;

    class InflectionProcess
    {
        private readonly string singularEnding;
        private readonly string pluralEnding;

        private InflectionProcess(string singularEnding, string pluralEnding)
        {
            this.singularEnding = singularEnding;
            this.pluralEnding = pluralEnding;
        }

        public bool MatchSingularForPlural(string singular) => singular.EndsWith(singularEnding);
        public string InflectSingularForPlural(string singular) =>
            singular.Substring(0, singular.Length - singularEnding.Length) + pluralEnding;

        public bool MatchPluralForSingular(string plural) => plural.EndsWith(pluralEnding);
        public string InflectPluralForSingular(string plural) =>
            plural.Substring(0, plural.Length - pluralEnding.Length) + singularEnding;

        public static InflectionProcess[] FromFile(string path)
        {
            var lines = File.ReadAllLines(path).Where(line => !line.StartsWith("#"));
            var columns = lines.Select(line => line.Split('\t'));
            return columns.Select(line => new InflectionProcess(line[0], line[1])).ToArray();
        }
    }

    public static string[] PassiveParticiple(string[] baseForm)
    {
        if (baseForm.Length == 1 && irregularVerbs.LookupOrNull(baseForm[0], "Passive participle") is string irregular)
        {
            return new string[] {irregular};
        }

        var passive = (string[]) baseForm.Clone();
        var end = passive.Length - 1;
        var last = passive[end];
        var len = last.Length;

        if (last.EndsWith("e"))
            last = last + "d";
        if (last.EndsWith("y") && len > 1 && IsConsonant(last[len - 2]))
            last = last.Substring(0, len - 1) + "ied";
        else if (last.EndsWith("c"))
            last = last + "ked";
        else if (IsConsonant(last[len - 1]) && last[len-1] != 'y' && len > 1 && IsVowel(last[len - 2]))
            last = last + last[len - 1] + "ed";
        else
            last = last + "ed";

        passive[end] = last;

        return passive;
    }
}
