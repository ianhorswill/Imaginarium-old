using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Implements methods for scanning input tokens and backtracking.
/// </summary>
public static class Parser
{
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
    /// Finds the matching Syntax rule for sentence and runs its associated action.
    /// </summary>
    /// <param name="sentence">User input (either an ontology statement or a command)</param>
    /// <returns>True if command altered the ontology.</returns>
    public static bool ParseAndExecute(string sentence)
    {
        sentence = sentence.TrimEnd(' ', '.');
        // Load text
        Input.Clear();
        Input.AddRange(Tokenizer.Tokenize(sentence));

        var rule = Syntax.AllRules.FirstOrDefault(r => r.Try());

        // Parse!
        if (rule == null)
            throw new GrammaticalError("Unknown sentence form", sentence);

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
        Syntax.ResetConstituentInformation();

        // Initialize state
        currentTokenIndex = 0;
        UndoStack.Clear();
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

        if (string.Equals(CurrentToken, token, StringComparison.OrdinalIgnoreCase))
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

        return false;
    }

    /// <summary>
    /// Attempts to match tokens to the name of a known monadic concept (CommonNoun or Adjective)
    /// </summary>
    /// <returns>The concept, if successful, or null</returns>
    public static TReferent MatchTrie<TReferent>(TokenTrie<TReferent> trie)
        where TReferent : Referent
    {
        var old = State;
        var concept = trie.Lookup(Input, ref currentTokenIndex);
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
    public static readonly List<string> Input = new List<string>();

    /// <summary>
    /// Index within input of the next token to be matched
    /// </summary>
    private static int currentTokenIndex;

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
    private static readonly Stack<Action> UndoStack = new Stack<Action>();

    public static void UndoAction(Action a)
    {
        UndoStack.Push(a);
    }

    /// <summary>
    /// Current state of the parser.
    /// </summary>
    public static ParserState State => new ParserState(currentTokenIndex, UndoStack.Count);

    public struct ParserState
    {
        public readonly int CurrentTokenIndex;
        public readonly int UndoStackDepth;

        public ParserState(int currentTokenIndex, int undoStackDepth)
        {
            CurrentTokenIndex = currentTokenIndex;
            UndoStackDepth = undoStackDepth;
        }
    }

    public static void ResetTo(ParserState s)
    {
        currentTokenIndex = s.CurrentTokenIndex;
        while (UndoStack.Count != s.UndoStackDepth)
            UndoStack.Pop()();
    }
    #endregion

    #region Definition files
    /// <summary>
    /// Directory holding definitions files and item lists.
    /// </summary>
    public static string DefinitionsDirectory;

    /// <summary>
    /// Returns full path for library definitions for the specified noun.
    /// </summary>
    public static string DefinitionFilePath(Referent referent)
    {
        var fileName = referent.Text;
        return DefinitionFilePath(fileName);
    }

    /// <summary>
    /// Returns the full path for the specified file in the definition library.
    /// </summary>
    public static string DefinitionFilePath(string fileName)
    {
        var definitionFilePath = Path.Combine(DefinitionsDirectory, fileName + ".txt");
        return definitionFilePath;
    }

    /// <summary>
    /// Load definitions for noun, if there is a definition file for it.
    /// Called when noun is first added to ontology.
    /// </summary>
    public static void MaybeLoadDefinitions(Referent referent)
    {
        if (DefinitionsDirectory != null && File.Exists(DefinitionFilePath(referent)))
            LoadDefinitions(referent);
    }

    /// <summary>
    /// Add all the statements from the definition file for noun to the ontology
    /// </summary>
    public static void LoadDefinitions(Referent referent)
    {
        foreach (var def in File.ReadAllLines(DefinitionFilePath(referent)))
        {
            var trimmed = def.Trim();
            if (trimmed != "" && !trimmed.StartsWith("#"))
                ParseAndExecute(trimmed);
        }
    }
    #endregion
}