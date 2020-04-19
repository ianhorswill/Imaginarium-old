#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Parser.cs" company="Ian Horswill">
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
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Implements methods for scanning input tokens and backtracking.
/// </summary>
public static class Parser
{
    /// <summary>
    /// Files from the current project (generator) currently being loaded
    /// </summary>
    public static List<string> LoadedFiles = new List<string>();
    /// <summary>
    /// File currently being loaded
    /// </summary>
    public static string CurrentSourceFile;
    /// <summary>
    /// Line of the file we're currently loading
    /// </summary>
    public static int CurrentSourceLine;
    /// <summary>
    /// The update time of the most recently loaded file.
    /// </summary>
    public static DateTime MostRecentFileUpdateTime;
    
    /// <summary>
    /// Parse and execute a new command from the user, and log it if it's an ontology alteration
    /// </summary>
    /// <param name="command"></param>
    public static void UserCommand(string command)
    {
        if (ParseAndExecute(command))
            History.Log(command);
    }

    /// <summary>
    /// Rule in which grammatical error was detected.
    /// </summary>
    public static Syntax RuleTriggeringException;

    public static string InputTriggeringException;

    /// <summary>
    /// Finds the matching Syntax rule for sentence and runs its associated action.
    /// </summary>
    /// <param name="sentence">User input (either an ontology statement or a command)</param>
    /// <returns>True if command altered the ontology.</returns>
    public static bool ParseAndExecute(string sentence)
    {
        sentence = sentence.TrimEnd(' ', '.');
        InputTriggeringException = sentence;
        // Load text
        Input.Clear();
        Input.AddRange(Tokenizer.Tokenize(sentence));

        RuleTriggeringException = null;
        var rule = Syntax.AllRules.FirstOrDefault(r =>
        {
            RuleTriggeringException = r;
            return r.Try();
        });
        RuleTriggeringException = null;

        if (rule == null)
            throw new GrammaticalError($"Unknown sentence pattern: {sentence}", 
                "This doesn't match any of the sentence patterns I know");

        InputTriggeringException = null;

        return !rule.IsCommand;
    }

    /// <summary>
    /// Parse and execute a series of statements
    /// </summary>
    public static void ParseAndExecute(params string[] statements)
    {
        foreach (var sentence in statements)
            ParseAndExecute(sentence);
    }

    /// <summary>
    /// Re-initializes all information associated with parsing.
    /// </summary>
    public static void ResetParser()
    {
        Current.ResetConstituentInformation();

        // Initialize state
        currentTokenIndex = 0;
    }

    #region Token matching
    /// <summary>
    /// Attempt to match next token to TOKEN.  If successful, returns true and advances to next token.
    /// </summary>
    /// <param name="token">Token to match to next token in input</param>
    /// <returns>Success</returns>
    public static bool Match(string token)
    {
        if (EndOfInput)
            return false;

        if (String.Equals(CurrentToken, token, StringComparison.OrdinalIgnoreCase))
        {
            currentTokenIndex++;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to match the specified series of tokens.
    /// Each token must match in order.  If any token fails to match, state is reset
    /// to the state before the call.
    /// </summary>
    /// <param name="tokens">Tokens to match</param>
    /// <returns>True on success</returns>
    public static bool Match(params string[] tokens)
    {
        var s = State;
        foreach (var t in tokens)
            if (!Match(t))
            {
                ResetTo(s);
                return false;
            }

        return true;
    }

    /// <summary>
    /// Attempt to match next token to TOKEN.  If successful, returns true and advances to next token.
    /// </summary>
    /// <param name="tokenPredicate">Predicate to apply to next token</param>
    /// <returns>Success</returns>
    public static bool Match(Func<string, bool> tokenPredicate)
    {
        if (tokenPredicate(CurrentToken))
        {
            currentTokenIndex++;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempt to match token to a conjugation of "be"
    /// </summary>
    /// <returns>True on success</returns>
    public static bool MatchCopula()
    {
        if (Match("is"))
        {
            Syntax.VerbNumber = Syntax.Number.Singular;
            return true;
        }
        if (Match("are"))
        {
            Syntax.VerbNumber = Syntax.Number.Plural;
            return true;
        }

        return false;
    }

    /// <summary>
    /// True if argument is a conjugation of "be"
    /// </summary>
    public static bool IsCopula(string s)
    {
        return s == "is" || s == "are";
    }

    /// <summary>
    /// Attempt to match token to a conjugation of "have"
    /// </summary>
    /// <returns>True on success</returns>
    public static bool MatchHave()
    {
        if (Match("has"))
        {
            Syntax.VerbNumber = Syntax.Number.Singular;
            return true;
        }
        if (Match("have"))
        {
            Syntax.VerbNumber = Syntax.Number.Plural;
            return true;
        }

        return false;
    }

    /// <summary>
    /// True if argument is a conjugation of "have"
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static bool IsHave(string s)
    {
        return s == "has" || s == "have";
    }

    private static readonly string[] NumberWords =
        { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten"};
    /// <summary>
    /// Attempt to match token to a number.  If successful, writes number to out arg.
    /// </summary>
    /// <param name="number">Variable or field to write result back to</param>
    /// <returns>True on success</returns>
    public static bool MatchNumber(out float number)
    {
        var token = CurrentToken;
        if (float.TryParse(token, out number))
        {
            SkipToken();
            return true;
        }

        number = Array.IndexOf(NumberWords, token);

        var success = number >= 0;
        if (success)
            SkipToken();

        return success;
    }

    /// <summary>
    /// Attempts to match tokens to the name of a known monadic concept (CommonNoun or Adjective)
    /// </summary>
    /// <returns>The concept, if successful, or null</returns>
    public static TReferent MatchTrie<TReferent>(TokenTrie<TReferent> trie)
        where TReferent : Referent
    {
        var old = State;
        var concept = trie.Lookup(Input, ref Current.CurrentTokenIndex);
        if (concept != null)
            return concept;
        ResetTo(old);
        return null;
    }

    /// <summary>
    /// Skip to the next token in the input
    /// </summary>
    public static void SkipToken()
    {
        if (EndOfInput)
            throw new InvalidOperationException("Attempt to skip past end of input");
        currentTokenIndex++;
    }

    /// <summary>
    /// Skip over all remaining tokens, to the end of the input.
    /// </summary>
    public static void SkipToEnd()
    {
        currentTokenIndex = Input.Count;
    }

    /// <summary>
    /// "Unread" the last token
    /// </summary>
    public static void Backup()
    {
        currentTokenIndex--;
    }
    #endregion

    #region State variables
    /// <summary>
    /// List of tokens to be parsed
    /// </summary>
    public static List<string> Input => Current.TokenStream;

    /// <summary>
    /// Index within input of the next token to be matched
    /// </summary>
    // ReSharper disable once InconsistentNaming
    private static int currentTokenIndex
    {
        get => Current.CurrentTokenIndex;
        set => Current.CurrentTokenIndex = value;
    }

    /// <summary>
    /// True if all tokens have already been read
    /// </summary>
    public static bool EndOfInput => currentTokenIndex == Input.Count;
    /// <summary>
    /// Token currently being processed.
    /// Fails if EndOfInput.
    /// </summary>
    public static string CurrentToken => Input[currentTokenIndex];
    #endregion

    #region State maintenance
    /// <summary>
    /// Current state of the parser.
    /// </summary>
    public static ScannerState State => new ScannerState(currentTokenIndex);

    public static string RemainingInput
    {
        get
        {
            var b = new StringBuilder();
            for (var i = currentTokenIndex; i < Input.Count; i++)
            {
                b.Append(Input[i]);
                b.Append(' ');
            }

            return b.ToString();
        }
    }

    public struct ScannerState
    {
        public readonly int CurrentTokenIndex;

        public ScannerState(int currentTokenIndex)
        {
            CurrentTokenIndex = currentTokenIndex;
        }
    }

    public static void ResetTo(ScannerState s)
    {
        currentTokenIndex = s.CurrentTokenIndex;
    }

    public static Stack<ParserState> Parsers = new Stack<ParserState>();

    public static ParserState Current = new ParserState();

    public static void Push()
    {
        Parsers.Push(Current);
        Current = new ParserState();
    }

    public static void Pop()
    {
        Current = Parsers.Pop();
    }

    public class ParserState
    {
        public ParserState()
        {
            ResetConstituentInformation();
        }

        /// <summary>
        /// Reinitialize global variables that track the values of constituents.
        /// Called each time a new syntax rule is tried.
        /// </summary>
        public void ResetConstituentInformation()
        {
            Subject.Reset();
            Verb.Reset();
            Verb2.Reset();
            Object.Reset();
            PredicateAP.Reset();
            SubjectNounList.Reset();
            PredicateAPList.Reset();
            VerbNumber = null;
        }

        public readonly List<string> TokenStream = new List<string>();
        public int CurrentTokenIndex;

        /// <summary>
        /// Segment for the subject of a sentence
        /// </summary>
        public NP Subject = new NP() {Name = "Subject"};

        /// <summary>
        /// Segment for the object of a sentence
        /// </summary>
        public NP Object = new NP() {Name = "Object"};

        public VerbSegment Verb = new VerbSegment() {Name = "Verb"};
        public VerbSegment Verb2 = new VerbSegment() {Name = "Verb2"};

        /// <summary>
        /// Used when the subject of a sentences is a list of NPs
        /// </summary>
        public ReferringExpressionList<NP, Noun> SubjectNounList = new ReferringExpressionList<NP, Noun>()
            {SanityCheck = Syntax.ForceBaseForm, Name = "Subjects"};

        /// <summary>
        /// Used when the predicate of a sentences is a list of APs
        /// </summary>
        public ReferringExpressionList<AP, Adjective> PredicateAPList =
            new ReferringExpressionList<AP, Adjective>()
                {Name = "Adjectives"};

        /// <summary>
        /// Segment for the AP forming the predicate of a sentences
        /// </summary>
        public AP PredicateAP = new AP() {Name = "Adjective"};

        /// <summary>
        /// Segment for the file name of a list of values (e.g. for possible names of characters)
        /// </summary>
        public Segment ListName = new Segment() {Name = "ListName"};

        /// <summary>
        /// Segment for the name of a button being created
        /// </summary>
        public Segment ButtonName = new Segment() {Name = "ButtonName"};
        
        /// <summary>
        /// Free-form text, e.g. from a quotation.
        /// </summary>
        public Segment Text = new Segment() {Name = "AnyText", AllowListConjunctions = true };

        public QuantifyingDeterminer Quantifier = new QuantifyingDeterminer() {Name = "one/many/other"};

        /// <summary>
        /// The lower bound of a range appearing in the definition of a numeric property
        /// </summary>
        public float LowerBound;

        /// <summary>
        /// The upper bound of a range appearing in the definition of a numeric property
        /// </summary>
        public float UpperBound;
        
        public Syntax.Number? VerbNumber;
    }

    #endregion

    #region Definition files

    /// <summary>
    /// Directory holding definitions files and item lists.
    /// </summary>
    public static string DefinitionsDirectory
    {
        get => _definitionsDirectory;
        set
        {
            _definitionsDirectory = value;
            MostRecentFileUpdateTime = DateTime.MinValue;
            // Throw away our state when we change projects
            History.Clear();
        }
    }

    private static string _definitionsDirectory;

    /// <summary>
    /// Returns full path for library definitions for the specified noun.
    /// </summary>
    public static string DefinitionFilePath(Referent referent)
    {
        var fileName = referent.Text;
        return DefinitionFilePath(fileName);
    }

    public static bool NameIsValidFilename(Referent referent) =>
        NameIsValidFilename(referent.Text);

    public static bool NameIsValidFilename(string fileName) =>
        fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

    /// <summary>
    /// Returns the full path for the specified file in the definition library.
    /// </summary>
    public static string DefinitionFilePath(string fileName) =>
        Path.Combine(DefinitionsDirectory, fileName + ConfigurationFiles.SourceExtension);

    /// <summary>
    /// Returns the full path for the specified list file in the definition library.
    /// </summary>
    public static string ListFilePath(string fileName)
    {
        var definitionFilePath = Path.Combine(DefinitionsDirectory, fileName + ConfigurationFiles.ListExtension);
        return definitionFilePath;
    }

    /// <summary>
    /// Load definitions for noun, if there is a definition file for it.
    /// Called when noun is first added to ontology.
    /// </summary>
    public static void MaybeLoadDefinitions(Referent referent)
    {
        if (DefinitionsDirectory != null && NameIsValidFilename(referent) && File.Exists(DefinitionFilePath(referent)))
            LoadDefinitions(referent);
    }

    /// <summary>
    /// Add all the statements from the definition file for noun to the ontology
    /// </summary>
    public static void LoadDefinitions(Referent referent)
    {
        var path = DefinitionFilePath(referent);
        LoadDefinitions(path);
    }

    public static void LoadDefinitions(string path)
    {
        if (LoadedFiles.Contains(path))
            return;

        LogFile.Log("Loading " + path);
        LoadedFiles.Add(path);
        var oldPath = CurrentSourceFile;
        var oldLine = CurrentSourceLine;
        CurrentSourceFile = path;
        CurrentSourceLine = 0;

        var update = File.GetLastWriteTimeUtc(path);
        if (update > MostRecentFileUpdateTime)
            MostRecentFileUpdateTime = update;

        Push();

        var assertions = File.ReadAllLines(path);
        foreach (var def in assertions)
        {
            CurrentSourceLine++;
            var trimmed = def.Trim();
            if (trimmed != "" && !trimmed.StartsWith("#") && !trimmed.StartsWith("//"))
                ParseAndExecute(trimmed);
        }

        Pop();
        LogFile.Log("Finished loading of " + path);
        CurrentSourceFile = oldPath;
        CurrentSourceLine = oldLine;
    }

    #endregion
}