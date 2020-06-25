#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="History.cs" company="Ian Horswill">
// Copyright (C) 2019, 2020 Ian Horswill
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
using Imaginarium.Driver;
using Imaginarium.Generator;
using Imaginarium.Parsing;
using File = System.IO.File;

/// <summary>
/// Maintains a log of declarations entered by the user.
/// </summary>
public class History
{
    public readonly UIDriver UIDriver;

    private readonly List<string> declarations = new List<string>();

    public History(UIDriver uiDriver)
    {
        UIDriver = uiDriver;
    }

    /// <summary>
    /// Add a declaration to the log
    /// </summary>
    /// <param name="declaration"></param>
    public void Log(string declaration)
    {
        declarations.Add(declaration);
    }

    /// <summary>
    /// Remove the last declaration from the log and rebuild the ontology.
    /// </summary>
    /// <returns>The declaration removed from the log</returns>
    public string Undo()
    {
        string undone = null;
        if (declarations.Count > 0)
        {
            var end = declarations.Count - 1;
            undone = declarations[end];
            declarations.RemoveAt(end);
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
    private void Replay()
    {
        UIDriver.ClearButtons();
        UIDriver.Ontology.Reload();
        var p = new Parser(UIDriver.Ontology);
        foreach (var decl in declarations)
            p.ParseAndExecute(decl);
    }

    public void Save(string path)
    {
        File.WriteAllLines(path, declarations);
        LogFile.Log("Saving to "+path);
        foreach (var line in declarations)
            LogFile.Log("   "+ line);
    }

    public void Clear()
    {
        declarations.Clear();
        Replay();
        Generator.Current = null;
    }
}