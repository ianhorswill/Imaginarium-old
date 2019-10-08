using System.Collections.Generic;
using System.IO;
using UnityEngine.Windows;
using File = System.IO.File;

/// <summary>
/// Maintains a log of declarations entered by the user.
/// </summary>
public static class History
{
    private static readonly List<string> Declarations = new List<string>();

    /// <summary>
    /// Add a declaration to the log
    /// </summary>
    /// <param name="declaration"></param>
    public static void Log(string declaration)
    {
        Declarations.Add(declaration);
    }

    /// <summary>
    /// Remove the last declaration from the log and rebuild the ontology.
    /// </summary>
    /// <returns>The declaration removed from the log</returns>
    public static string Undo()
    {
        string undone = null;
        if (Declarations.Count > 0)
        {
            var end = Declarations.Count - 1;
            undone = Declarations[end];
            Declarations.RemoveAt(end);
        }

        Replay();
        Generator.Current = null;
        return undone;
    }

    /// <summary>
    /// Rerun the declarations that were not undone.
    /// </summary>
    private static void Replay()
    {
        Ontology.EraseConcepts();
        foreach (var decl in Declarations)
            Parser.ParseAndExecute(decl);
    }

    public static void Save(string path)
    {
        File.WriteAllLines(path, Declarations);
        LogFile.Log("Saving to "+path);
        foreach (var line in Declarations)
            LogFile.Log("   "+ line);
    }

    public static void Edit(string path)
    {
        //Ontology.EraseConcepts();
        //Parser.LoadDefinitions(path);
        Declarations.Clear();
        Declarations.AddRange(File.ReadAllLines(path));
    }

    public static void Clear()
    {
        Declarations.Clear();
        Replay();
        Generator.Current = null;
    }
}