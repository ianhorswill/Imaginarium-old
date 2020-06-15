#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Ontology.cs" company="Ian Horswill">
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

/// <summary>
/// Operations for accessing the ontology as a whole
/// The ontology consists of all the Referent objects and the information within them (e.g. Property objects)
/// </summary>
[DebuggerDisplay("{" + nameof(Name) + "}")]
public class Ontology
{
    public Ontology(string name)
    {
        AllReferentTables.Add(AllAdjectives);
        AllReferentTables.Add(AllPermanentIndividuals);
        AllReferentTables.Add(AllNouns);
        AllReferentTables.Add(AllParts);
        AllReferentTables.Add(AllProperties);

        VerbTrie = new TokenTrie<Verb>(this);
        MonadicConceptTrie = new TokenTrie<MonadicConcept>(this);
        Name = name;
    }

    public readonly string Name;

    public readonly List<TokenTrieBase> AllTokenTries = new List<TokenTrieBase>();

    /// <summary>
    /// List of all the tables of different kinds of referents.
    /// Used so we know what to clear when reinitializing the ontology.
    /// </summary>
    public readonly List<IDictionary> AllReferentTables = new List<IDictionary>();

    internal readonly Dictionary<TokenString, Adjective> AllAdjectives = new Dictionary<TokenString, Adjective>();

    /// <summary>
    /// List of all common nouns (i.e. kinds/types) in the ontology.
    /// </summary>
    public IEnumerable<CommonNoun> AllCommonNouns => AllNouns.Select(pair => pair.Value)
        .Where(n => n is CommonNoun).Cast<CommonNoun>().Distinct();


    public Dictionary<TokenString, Individual> AllPermanentIndividuals = new Dictionary<TokenString, Individual>();

    public readonly TokenTrie<MonadicConcept> MonadicConceptTrie;
    public bool LastMatchPlural => MonadicConceptTrie.LastMatchPlural;

    public Dictionary<TokenString, Noun> AllNouns = new Dictionary<TokenString, Noun>();

    internal readonly Dictionary<TokenString, Part> AllParts = new Dictionary<TokenString, Part>();

    internal readonly Dictionary<TokenString, Property> AllProperties = new Dictionary<TokenString, Property>();

    public readonly TokenTrie<Verb> VerbTrie;

    public IEnumerable<Verb> AllVerbs => VerbTrie.Contents.Distinct();

    public void ClearAllTries()
    {
        foreach (var t in AllTokenTries)
            t.Clear();
    }

    /// <summary>
    /// Return true if there's already a concept with the specified name.
    /// </summary>
    public object Find(TokenString name)
    {
        var dict = AllReferentTables.FirstOrDefault(t => t.Contains(name));
        var result = dict?[name];
        if (result == null)
            foreach (var t in AllTokenTries)
            {
                result = t.Find(name);
                if (result != null)
                    break;
            }

        return result;
    }

    /// <summary>
    /// Returns the adjective with the specified name, or null if none
    /// </summary>
    public Adjective FindAdjective(params string[] tokens) => AllAdjectives.LookupOrDefault(tokens);

    /// <summary>
    /// Returns the common noun identified by the specified sequence of tokens, or null, if there is no such noun.
    /// </summary>
    /// <param name="name">Tokens identifying the noun</param>
    /// <returns>Identified noun, or null if none found.</returns>
    public CommonNoun FindCommonNoun(params string[] name)
    {
        return (CommonNoun)FindNoun(name);
    }

    public Individual FindIndividual(params string[] tokens) => AllPermanentIndividuals.LookupOrDefault(tokens);

    /// <summary>
    /// Returns the noun named by the specified token string, or null if there is none.
    /// </summary>
    public Noun FindNoun(params string[] tokens) => AllNouns.LookupOrDefault(tokens);
    
    /// <summary>
    /// Return the property with the specified name, if any, otherwise null.
    /// </summary>
    public Part FindPart(params string[] tokens) => AllParts.LookupOrDefault(tokens);

    /// <summary>
    /// Return the property with the specified name, if any, otherwise null.
    /// </summary>
    public Property FindProperty(params string[] tokens) => AllProperties.LookupOrDefault(tokens);

    public Verb FindVerb(params string[] tokens)
    {
        int index = 0;
        var v = VerbTrie.Lookup(tokens, ref index);
        if (index != tokens.Length)
            return null;
        return v;
    }

    /// <summary>
    /// Add this name and concept to the trie of all known names of all known monadic concepts.
    /// </summary>
    /// <param name="tokens">Name to add for the concept</param>
    /// <param name="c">Concept to add</param>
    /// <param name="isPlural">True when concept is a common noun and the name is its plural.</param>
    public void Store(string[] tokens, MonadicConcept c, bool isPlural = false) => MonadicConceptTrie.Store(tokens, c, isPlural);

    /// <summary>
    /// Search trie for a monadic concept named by some substring of tokens starting at the specified index.
    /// Updates index as it searches
    /// </summary>
    /// <param name="tokens">Sequence of tokens to search</param>
    /// <param name="index">Position within token sequence</param>
    /// <returns>Concept, if found, otherwise null.</returns>
    public MonadicConcept Lookup(IList<string> tokens, ref int index) => MonadicConceptTrie.Lookup(tokens, ref index);

    /// <summary>
    /// Makes an Individual that is not part of the ontology itself.
    /// This individual is local to a particular Invention.
    /// </summary>
    /// <param name="concepts">CommonNouns and Adjectives that must apply to the individual</param>
    /// <param name="name">Default name to give to the individual if no name property can be found.</param>
    /// <param name="container">The object of which this is a part, if any</param>
    /// <param name="containerPart">Part of container which this object represents</param>
    /// <returns></returns>
    public Individual EphemeralIndividual(IEnumerable<MonadicConceptLiteral> concepts, string[] name, Individual container = null, Part containerPart = null)
    {
        return new Individual(this, concepts, name, container, containerPart);
    }

    /// <summary>
    /// Makes an Individual that is part of the ontology itself.  It will appear in all Inventions.
    /// </summary>
    /// <param name="concepts">CommonNouns and Adjectives that must be true of this Individual</param>
    /// <param name="name">Default name for the individual if not name property can be found.</param>
    /// <returns></returns>
    public Individual PermanentIndividual(IEnumerable<MonadicConceptLiteral> concepts, string[] name)
    {
        var individual = new Individual(this, concepts, name);
        AllPermanentIndividuals[name] = individual;
        return individual;
    }


    /// <summary>
    /// Removes all concepts form the ontology.
    /// </summary>
    public void EraseConcepts()
    {
        foreach (var c in AllReferentTables)
            c.Clear();
        
        ClearAllTries();
        Parser.LoadedFiles.Clear();
        tests.Clear();
    }

    /// <summary>
    /// Reload the current project
    /// </summary>
    public void Reload()
    {
        EraseConcepts();
        Load();
    }

    /// <summary>
    /// Load all the source files in the current project
    /// </summary>
    private void Load()
    {
        Driver.ClearLoadErrors();

        foreach (var file in Directory.GetFiles(DefinitionsDirectory))
            if (!Path.GetFileName(file).StartsWith(".")
                && Path.GetExtension(file) == ConfigurationFiles.SourceExtension)
            {
                try
                {
                    var p = new Parser(this);
                    p.LoadDefinitions(file);
                }
                catch (Exception e)
                {
                    Driver.LogLoadError(Parser.CurrentSourceFile, Parser.CurrentSourceLine, e.Message);
                    throw;
                }
            }
    }


    /// <summary>
    /// Directory holding definitions files and item lists.
    /// </summary>
    public string DefinitionsDirectory
    {
        get => _definitionsDirectory;
        set
        {
            _definitionsDirectory = value;
            // Throw away our state when we change projects
            EraseConcepts();
        }
    }

    private static string _definitionsDirectory;

    public void EnsureUndefinedOrDefinedAsType(string[] name, Type newType)
    {
        if (name == null)
            return;
        var old = Find((TokenString) name);
        if (old != null && old.GetType() != newType)
            throw new NameCollisionException(name, old.GetType(), newType);
    }

    #region Testing
    private readonly List<Test> tests = new List<Test>();

    public void ClearTests()
    {
        tests.Clear();
    }
    
    public void AddTest(CommonNoun noun, IEnumerable<MonadicConceptLiteral> modifiers, bool shouldExist, string succeedMessage, string failMessage)
    {
        tests.Add(new Test(noun, modifiers, shouldExist, succeedMessage, failMessage));
    }

    public IEnumerable<(Test test, bool success, Invention example)> TestResults()
    {
        foreach (var test in tests)
        {
            var (success, example) = test.Run();
            yield return (test, success, example);
        }
    }
    #endregion
}
