using System.Collections.Generic;
using System.Linq;
using File = System.IO.File;

/// <summary>
/// Implements a best effort to convert between English plural and singular noun inflections
/// </summary>
public static class Inflection
{
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
        throw new GrammaticalError(singular+" appears to be a singular noun, but I can't find a plural inflection for it");
    }

    public static string SingularOfNoun(string plural)
    {
        if (IrregularSingulars.TryGetValue(plural, out string singular))
            return singular;
        foreach (var i in Inflections)
            if (i.MatchPluralForSingular(plural))
                return i.InflectPluralForSingular(plural);
        throw new GrammaticalError(plural+" appears to be a plural noun, but I can't find a singular inflection for it");
    }

    public static string[] SingularOfNoun(string[] plural)
    {
        var singular = new string[plural.Length];
        plural.CopyTo(singular,0);
        var last = plural.Length - 1;
        singular[last] = SingularOfNoun(plural[last]);
        return singular;
    }

    public static string[] SingularOfVerb(string[] plural) => PluralOfNoun(plural);
    public static string[] PluralOfVerb(string[] singular) => SingularOfNoun(singular);

    private static readonly Dictionary<string, string> IrregularPlurals = new Dictionary<string, string>();

    private static readonly  Dictionary<string, string> IrregularSingulars = new Dictionary<string, string>();

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
    }

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
}
