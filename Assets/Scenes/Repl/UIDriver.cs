#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UIDriver.cs" company="Ian Horswill">
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

using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GraphVisualization;
using Imaginarium.Driver;
using Imaginarium.Generator;
using Imaginarium.Ontology;
using Imaginarium.Parsing;

public class UIDriver : MonoBehaviour, IRepl
{
    public static readonly Ontology Ontology = new Ontology("Default", null);

    public static Invention Invention;
    private List<string> inventionDescriptions;
    public InputField InputField;
    public Text OutputField;
    public ScrollRect OutputScrollRect;
    public Scrollbar OutputVerticalScroll;
    public ContentSizeFitter OutputFitter;
    public Graph RelationshipGraph;
    public Transform ButtonBarContent;
    public GameObject ButtonPrefab;

    public Parser Parser;
    public History History;

    /// <summary>
    /// Called at startup.
    /// Initialize UI system.
    /// </summary>
    public IEnumerator Start()
    {
        DataFiles.DataHome = Application.dataPath;
        Parser = new Parser(Ontology, parser => ReplCommands.Commands(parser, this));
        Driver.Repl = this;
        History = new History(this);
        ConfigurationFiles.UnityPath = Application.dataPath;

        var generator = PlayerPrefs.GetString("DefinitionsDirectory", null);

        if (String.IsNullOrEmpty(generator))
            OutputField.text = "No generator selected.  Press <b>F1 for help</b>, ESC for menu.";
        else
        {
            OutputField.text = $"<size=120>Using <b>{Path.GetFileName(generator)}</b> generator.</size>\nPress <b>F1 for help</b>, ESC for menu.";
            try
            {
                Ontology.DefinitionsDirectory = generator;
                Ontology.Reload();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            CheckForLoadErrors();
        }

        // Move keyboard focus to input
        SelectInput();
        return ScrollToTop();
    }

    private void CheckForLoadErrors()
    {
        var loadErrors = Driver.LoadErrors;
        if (loadErrors != null)
        {
            OutputField.text = $"{loadErrors}\n{BuildVersionMessage}";
            Driver.ClearLoadErrors();
        }
    }

    private static string BuildVersionMessage =>
        $"This is <i>Imaginarium</i> version {Application.version}"; 

    /// <summary>
    /// Called when this UI mode is disabled.
    /// </summary>
    public void OnDisable()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// Move keyboard focus to input field
    /// </summary>
    private void SelectInput()
    {
        InputField.Select();
        InputField.ActivateInputField();
    }

    /// <summary>
    /// Input typed by user
    /// </summary>
    private string Input
    {
        get => InputField.text;
        set => InputField.text = value;
    }

    /// <summary>
    /// Output to present to user
    /// </summary>
    private string Output
    {
        get => OutputField.text;
        set
        {
            OutputField.text = value;
            StartCoroutine(ScrollToTop());
        }
    }

    /// <summary>
    /// Scroll the output area to the top of the screen
    /// </summary>
    /// <returns></returns>
    IEnumerator ScrollToTop()
    {
        // Wait for ContentSizeFitter to recalculate size of Output
        yield return null;
        // Wait for ScrollRect to notice size has changed
        yield return null;
        // Update scroll
        OutputScrollRect.verticalNormalizedPosition = 1;
        // Wait a frame for because scrolling moves focus
        yield return null;
        // Move focus back to input
        SelectInput();
    }

    /// <summary>
    /// Height of the ScrollRect
    /// </summary>
    private float ViewportSize => OutputScrollRect.GetComponent<RectTransform>().rect.height;
    /// <summary>
    /// Total height of the text being displayed
    /// </summary>
    private float OutputSize => OutputField.GetComponent<RectTransform>().rect.height;

    /// <summary>
    /// Scroll down the specified number of pages
    /// </summary>
    /// <param name="pages"></param>
    public void ScrollPages(int pages)
    {
        var pageSize = ViewportSize / OutputSize;
        OutputScrollRect.verticalNormalizedPosition = Mathf.Clamp(OutputScrollRect.verticalNormalizedPosition - pages*pageSize, 0, 1);
    }

    /// <summary>
    /// There must be a better way of doing this in the new UI
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private void OnGUI()
    {
        //Scenes.HandleSceneKeys(Scenes.Menu);

        var e = Event.current;

        if (e.type == EventType.KeyDown)
        {
            switch (e.keyCode)
            {
                case KeyCode.PageDown:
                    ScrollPages(1);
                    break;

                case KeyCode.PageUp:
                    ScrollPages(-1);
                    break;

                case KeyCode.Insert:
                    GUIUtility.systemCopyBuffer = Output;
                    break;
            }
        }
    }

    public void DoCommand()
    {
        // Not sure how it is this can get called from SetActive,
        // but it turns out that it can and that breaks the attempt to launch
        // the focus co-routine.
        if (!gameObject.activeSelf)
            return;

        if (!UnityEngine.Input.GetKeyDown(KeyCode.Return))
            return;

        LogFile.Separate();
        LogFile.Log("USER COMMAND");
        LogFile.Log("> "+Input);
        Driver.ClearCommandBuffer();

        try
        {
            if (Input != "")
                UserCommand(Input);
            MaybeRegenerateInvention();

            Input = "";
        }
        catch (Exception ex)
        {
            var previousOutput = Driver.CommandResponse;
            Driver.ClearCommandBuffer();
            Driver.AppendResponseLine("<color=yellow>");
            Driver.AppendResponseLine(Parser.RuleTriggeringException == null
                ? $"Uh oh.  I got confused by '<i>{Parser.InputTriggeringException ?? "none"}</i>'"
                : $"    Uh oh.  I got confused while matching the input '<i>{Parser.InputTriggeringException ?? "none"}</i>' to the pattern '{Parser.RuleTriggeringException.SentencePatternDescription}' ({Parser.RuleTriggeringException.SentencePatternDescription}).");
            Driver.AppendResponseLine($"    {FormatExceptionMessage(ex)}.");
            Driver.AppendResponseLine("</color>");

            if (ex is GrammaticalError && Parser.RuleTriggeringException == null)
            {
                var firstOne = true;
                foreach (var r in Parser.RulesMatchingKeywords(Parser.Input))
                {
                    if (firstOne)
                    {
                        Driver.AppendResponseLine("\n<b>    Perhaps you meant one of these sentence patterns:</b>\n");
                        firstOne = false;
                    }

                    Driver.AppendResponseLine($"{r.HelpDescription}\n");
                }
            }
            Driver.AppendResponseLine("");
            Driver.AppendResponseLine(previousOutput);

            LogFile.Log(ex.Message);
            LogFile.Log(ex.StackTrace);

            Driver.AppendResponseLine(BuildVersionMessage);
        }

        Output = Driver.CommandResponse;
    }

        
    /// <summary>
    /// Parse and execute a new command from the user, and log it if it's an ontology alteration
    /// </summary>
    /// <param name="command"></param>
    public void UserCommand(string command)
    {
        if (Parser.ParseAndExecute(command))
            History.Log(command);
    }

    private void MaybeRegenerateInvention()
    {
        if (Driver.CommandResponse == "" && Generator.Current != null)
        {
            Generator.Current.Rebuild();
            ReSolve();
            foreach (var s in inventionDescriptions)
                Driver.AppendResponseLine(s);

            MakeGraph();
        }
    }

    private static string FormatExceptionMessage(Exception ex)
    {
        string message;
        switch (ex)
        {
            case UserException e:
                message = e.RichText;
                break;

            default:
                message = $"Sorry.  An internal error ({ex.GetType().Name}) occurred: {ex.Message}";
                break;
        }

        if (ex.InnerException != null) 
            message += "\nInner exception:\n" + FormatExceptionMessage(ex.InnerException);

        return message;
    }

    private void ReSolve()
    {
        if (Generator.Current != null)
        {
            Invention = Generator.Current.Generate();
            if (LogFile.Enabled)
            {
                if (Invention == null)
                {
                    LogFile.Separate();
                    LogFile.Log("UNSATISFIABLE");
                    LogFile.Separate();
                }
                else
                {
                    LogFile.Separate();
                    LogFile.Log("MODEL");
                    LogFile.Log(Invention.Model.Model);
                    LogFile.Separate();
                    LogFile.Log("DESCRIPTION");
                    foreach (var i in Generator.Current.Individuals)
                        LogFile.Log(Invention.Description(i));
                }
            }

            if (Invention == null)
            {
                //Graph.Create();  // Remove existing graph, if any
                inventionDescriptions = new List<string> {"Can't think of any - maybe there's a contradiction?"};
            }
            else
            {
                inventionDescriptions = new List<string>(Generator.Current.Individuals.Count);

                var walked = new HashSet<Individual>();

                void Walk(Individual i)
                {
                    if (walked.Contains(i))
                        return;
                    walked.Add(i);

                    inventionDescriptions.Add(Invention.Description(i, "<b><color=#808080>", "</color></b>"));
                    foreach (var sub in Invention.Individuals)
                        if (sub.Container == i)
                            Walk(sub);
                }

                foreach (var i in Invention.Individuals)
                    Walk(i);

                //MakeGraph();
            }
        }
    }

    #region Relationship graph
    private int verbColorCounter;

    public static readonly Color[] Colors =
    {
        Color.green, Color.red, Color.blue, Color.gray, Color.cyan, Color.magenta, Color.yellow, Color.gray
    };

    private readonly Dictionary<Verb, EdgeStyle> verbColors = new Dictionary<Verb, EdgeStyle>();

    EdgeStyle VerbStyle(Verb v)
    {
        if (verbColors.TryGetValue(v, out var style))
            return style;
        style = RelationshipGraph.EdgeStyles[0].Clone();
        style.Color = Colors[verbColorCounter++ % Colors.Length];
        style.IsDirected = !v.IsSymmetric;
        verbColors[v] = style;
        return style;
    }

    private void MakeGraph()
    {
        RelationshipGraph.Clear();
        if (Invention == null)
            return;
        foreach (var i in Invention.Individuals) 
            RelationshipGraph.AddNode(i, Invention.NameString(i));
        foreach (var i in Invention.Individuals)
        foreach (var p in i.Parts)
            foreach (var pi in p.Value)
            RelationshipGraph.AddEdge(i, pi, p.Key.Text);

        foreach (var relationship in Invention.Relationships)
        {
            var v = relationship.Item1;
            var f = relationship.Item2;
            var t = relationship.Item3;
            var verb = v.Text;
            // Don't bother drawing if this is implied by a subspecies.
            if (v.Subspecies.Count == 0)
                RelationshipGraph.AddEdge(f, t, verb, VerbStyle(v));
        }
    }
    #endregion

    #region Button bar
    /// <summary>
    /// Adds a button to the button bar of the REPL
    /// </summary>
    /// <param name="buttonName">Text label for the button</param>
    /// <param name="command">Command to execute when the button is pressed.</param>
    public void AddButton(string buttonName, string command)
    {
        var button = Instantiate(ButtonPrefab, ButtonBarContent);
        button.name = buttonName;
        button.GetComponentInChildren<Text>().text = buttonName;
        button.GetComponent<Button>().onClick.AddListener(() =>
        {
            Driver.ClearCommandBuffer();
            Parser.ParseAndExecute(command);
            MaybeRegenerateInvention();
            Output = Driver.CommandResponse;
        });
    }

    public void ClearButtons()
    {
        ButtonBarContent.DestroyAllChildren();
    }
    #endregion

    #region Other IRepl stuff

    public void SetOutputWindow(string contents)
    {
        Output = contents;
    }
    #endregion
}
