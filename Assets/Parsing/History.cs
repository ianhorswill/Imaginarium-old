#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="History.cs" company="Ian Horswill">
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
            Driver.AppendResponseLine($"Undid {undone}");
        }
        else
            Driver.AppendResponseLine("No declarations to undo.");

        Replay();
        Generator.Current = null;
        return undone;
    }

    /// <summary>
    /// Rerun the declarations that were not undone.
    /// </summary>
    private static void Replay()
    {
        Driver.Ontology.Reload();
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