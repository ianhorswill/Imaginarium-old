#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Driver.cs" company="Ian Horswill">
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
using System.Linq;
using System.Text;
using UnityEngine;

public class Driver : MonoBehaviour
{
    public GUIStyle CommandStyle;
    public GUIStyle InventionStyle;
    public GUIStyle ErrorStyle;
    public GUIStyle OntologyStyle;
    private string command = "";

    private int counter;
    private Invention invention;
    private string[] inventionDescriptions;
    private float textAreaFraction = 0.5f;

    #region Command output
    private static readonly StringBuilder CommandBuffer = new StringBuilder();

    /// <summary>
    /// Remove any pending output
    /// </summary>
    public static void ClearCommandBuffer()
    {
        CommandBuffer.Length = 0;
    }

    public static void AppendResponseLine(string s)
    {
        CommandBuffer.AppendLine(s);
    }

    public static string CommandResponse => CommandBuffer.ToString();
    #endregion

    public void Start()
    {
        Parser.DefinitionsDirectory = Application.dataPath + "/Definitions";
    }

    public void OnGUI()
    {
        DrawGUI();
        ProcessEnter();
    }

    public int ScreenPadding = 50;

    private bool NeedGraph => Generator.Current != null && !Verb.Trie.IsEmpty;

    private void DrawGUI()
    {
        textAreaFraction = NeedGraph ? 0.6f : 1;
        GUILayout.BeginArea(TextAreaRect);
        GUILayout.Label("<b>Imaginarium</b>", CommandStyle);
        DrawCreation();
        DrawCommandLine();
        DrawLogControl();

        DrawOntology();

        GUILayout.EndArea();
    }

    private void DrawLogControl()
    {
        LogFile.Enabled = GUILayout.Toggle(LogFile.Enabled, "Debug log");
    }

    /// <summary>
    /// Rect in IMGUI coordinates of the textual part of the GUI
    /// </summary>
    public Rect TextAreaRect => new Rect(ScreenPadding, ScreenPadding,
        textAreaFraction*Screen.width - 2*ScreenPadding,
        Screen.height - 2*ScreenPadding);

    /// <summary>
    /// Rect in IMGUI coordinates of the graph area of the screen
    /// </summary>
    public Rect GraphAreaRect
    {
        get
        {
            var left = textAreaFraction * Screen.width;
            return new Rect(left, 0,
                Screen.width - left,
                Screen.height);
        }
    }

    private void DrawCreation()
    {
        if (Generator.Current != null)
            foreach (var d in inventionDescriptions)
                GUILayout.Label(d, InventionStyle);
        else
            GUILayout.Label("", InventionStyle);
        GUILayout.Space(20);
    }

    private void DrawCommandLine()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("> ", CommandStyle, GUILayout.Width(30));
        GUI.SetNextControlName("Command input");
        command = GUILayout.TextField(command, CommandStyle);
        GUI.FocusControl("Command input");
        GUILayout.EndHorizontal();
        GUILayout.Label(CommandResponse, ErrorStyle);

    }
    
    private void ProcessEnter()
    {
// Try to catch the return
        var e = Event.current;
        if (e.isKey &&
            ((e.keyCode == KeyCode.KeypadEnter) || (e.keyCode == KeyCode.Return)))
        {
            if (command == "")
            {
                LogFile.Separate();
                LogFile.Log("REGENERATING");
                ReSolve();
            }
            else
                DoCommand();

            LogFile.Flush();
        }
    }

    private void ReSolve()
    {
        if (Generator.Current != null)
        {
            invention = Generator.Current.Solve();
            if (LogFile.Enabled)
            {
                if (invention == null)
                {
                    LogFile.Separate();
                    LogFile.Log("UNSATISFIABLE");
                    LogFile.Separate();
                }
                else
                {
                    LogFile.Separate();
                    LogFile.Log("MODEL");
                    LogFile.Log(invention.Model.Model);
                    LogFile.Separate();
                    LogFile.Log("DESCRIPTION");
                    foreach (var i in Generator.Current.Individuals)
                        LogFile.Log(invention.Description(i));
                }
            }

            if (invention == null)
            {
                Graph.Create();  // Remove existing graph, if any
                inventionDescriptions = new[] { "Can't think of any - maybe there's a contradiction?"};
            }
            else
            {
                inventionDescriptions = new string[Generator.Current.Individuals.Count];
                for (var i = 0; i < Generator.Current.Individuals.Count; i++)
                    inventionDescriptions[i] =
                        invention.Description(Generator.Current.Individuals[i], "<b><color=grey>", "</color></b>");
                MakeGraph();
            }
        }
    }

    private int verbColorCounter;

    public static readonly string[] ColorNames =
    {
        "green", "red", "blue", "gray", "cyan", "magenta", "yellow", "gray"
    };

    private readonly Dictionary<Verb, string> verbColors = new Dictionary<Verb, string>();

    string VerbColor(Verb v)
    {
        if (verbColors.TryGetValue(v, out var c))
            return c;
        c = ColorNames[verbColorCounter++ % ColorNames.Length];
        verbColors[v] = c;
        return c;
    }

    private void MakeGraph()
    {
        Graph.Create();
        foreach (var i in invention.Individuals)
            Graph.SetColor(i, invention.NameString(i), "yellow");
        foreach (var (v, f, t) in invention.Relationships)
        {
            var from = invention.NameString(f);
            var to = invention.NameString(t);
            var verb = v.Text;
            Graph.AddEdge(f, from, t, to, verb, VerbColor(v));
        }
    }

    private void DoCommand()
    {
        LogFile.Separate();
        LogFile.Log("USER COMMAND");
        LogFile.Log("> "+command);
        AppendResponseLine("");
        try
        {
            Parser.UserCommand(command);
            if (Generator.Current != null)
            {
                Generator.Current.Rebuild();
                ReSolve();
            }
            command = "";
        }
        catch (GrammaticalError ex)
        {
            AppendResponseLine(ex.Message);
            LogFile.Log(ex.Message);
            LogFile.Log(ex.StackTrace);
        }
    }

    private void DrawOntology()
    {
        GUILayout.Label("<b>Nouns</b>", OntologyStyle);
        foreach (var common in CommonNoun.AllCommonNouns.ToArray())
            if (common.Superkinds.Count == 0)
                DrawSubtree(common, 0);
        GUILayout.Space(20);
        GUILayout.Label("<b>Verbs</b>", OntologyStyle);
        foreach (var verb in Verb.AllVerbs)
        {
            var subject = verb.SubjectKind == null?"null":verb.SubjectKind.PluralForm.Untokenize();
            var obj = verb.ObjectKind == null ? "null":verb.ObjectKind.PluralForm.Untokenize();
            GUILayout.Label($"{subject} can <b>{verb.PluralForm.Untokenize()}</b> {obj}", OntologyStyle);
        }
    }

    private void DrawSubtree(CommonNoun common, int indentLevel)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(20*indentLevel);
        DrawNounInfo(common);
        GUILayout.EndHorizontal();

        foreach (var sub in common.Subkinds)
            DrawSubtree(sub, indentLevel + 1);
    }

    private void DrawNounInfo(CommonNoun common)
    {
        try
        {
            var sing = common.SingularForm == null ? "Unknown singular form" : common.SingularForm.Untokenize();
            var plural = common.PluralForm == null ? "Unknown plural form" : common.PluralForm.Untokenize();
            Buffer.Length = 0;
            Buffer.Append($"<b>{sing}</B> ({plural})");
            if (common.Superkinds.Count > 0)
                AddList(" is a kind of", "and", common.Superkinds);

            foreach (var alt in common.AlternativeSets)
                AddList(alt.IsRequired ? ", must be" : ", can be", "or", alt.Alternatives.Select(a => a.Concept));

            if (common.Properties.Count > 0)
                AddList(", has properties", "and", common.Properties);

            if (common.ImpliedAdjectives.Count > 0)
                AddList(", is always", "and", common.ImpliedAdjectives.Select(i => i.Modifier.Concept));

            GUILayout.Label(Buffer.ToString(), OntologyStyle);
        }
        catch (Exception e)
        {
            GUILayout.Label(e.Message, OntologyStyle);
        }
    }

    private static readonly StringBuilder Buffer = new StringBuilder();

    private static void AddList(string header, string conjunction, IEnumerable<Referent> set)
    {
        Buffer.Append(header);
        AddList(conjunction, set);
    }

    private static void AddList(string conjunction, IEnumerable<Referent> set)
    {
        Referent previous = null;
        var alreadyPrinted = 0;
        foreach (var next in set)
        {
            if (previous != null)
            {
                if (alreadyPrinted == 0)
                    Buffer.Append(' ');
                else
                    Buffer.Append(", ");
                Buffer.Append(previous.Text);
                alreadyPrinted++;
            }
            previous = next;
        }

        if (previous != null)
        {
            if (alreadyPrinted < 2)
                Buffer.Append(' ');
            if (alreadyPrinted > 0)
            {
                Buffer.Append(conjunction);
                Buffer.Append(' ');
            }

            Buffer.Append(previous.Text);
        }
    }
}
