﻿using System;
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
    public static string CommandResponse = "";
    private int counter;
    private Invention invention;
    private string[] inventionDescriptions;
    private float textAreaFraction = 0.5f;

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

        DrawOntology();

        GUILayout.EndArea();
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
                ReSolve();
            else
                DoCommand();
        }
    }

    private void ReSolve()
    {
        if (Generator.Current != null)
        {
            var count = Generator.Current.Count;
            inventionDescriptions = new string[count];
            invention = Generator.Current.Solve();
            for (var i = 0; i < Generator.Current.Individuals.Count; i++)
                inventionDescriptions[i] =
                    invention.Description(Generator.Current.Individuals[i], "<b><color=grey>", "</color></b>");
            MakeGraph();
        }
    }

    private void MakeGraph()
    {
        Graph.Create();
        foreach (var i in invention.Individuals)
            Graph.SetColor(invention.NameString(i), "yellow");
        foreach (var (v, f, t) in invention.Relationships)
            if (f != t)
            {
                var from = invention.NameString(f);
                var to = invention.NameString(t);
                var verb = v.Text;
                Graph.AddEdge(from, to, verb, "green");
            }
    }

    private void DoCommand()
    {
        CommandResponse = "";
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
            CommandResponse = ex.Message;
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
                AddList(alt.IsRequired ? ", must be" : ", can be", "or", alt.Alternatives);

            if (common.Properties.Count > 0)
                AddList(", has properties", "and", common.Properties);

            if (common.ImpliedAdjectives.Count > 0)
                AddList(", is always", "and", common.ImpliedAdjectives.Select(i => i.Adjective));

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
