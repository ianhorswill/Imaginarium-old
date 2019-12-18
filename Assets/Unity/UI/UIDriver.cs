using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIDriver : MonoBehaviour
{
    private Invention invention;
    private string[] inventionDescriptions;
    private readonly StringBuilder buffer = new StringBuilder();
    public InputField InputField;
    public Text OutputField;
    public ScrollRect OutputScrollRect;
    public Scrollbar OutputVerticalScroll;
    public ContentSizeFitter OutputFitter;

    /// <summary>
    /// Called at startup.
    /// Initialize UI system.
    /// </summary>
    public void Start()
    {
        Parser.DefinitionsDirectory = Application.dataPath + "/Definitions";
        SelectInput();
    }

    /// <summary>
    /// Called when this UI mode is activated
    /// </summary>
    public void OnEnable()
    {
        // Move keyboard focus to input
        SelectInput();
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
            }
        }
    }

    public void DoCommand()
    {
        LogFile.Separate();
        LogFile.Log("USER COMMAND");
        LogFile.Log("> "+Input);
        Driver.CommandResponse = "";

        try
        {
            if (Input != "")
                Parser.UserCommand(Input);
            if (Driver.CommandResponse == "" && Generator.Current != null)
            {
                Generator.Current.Rebuild();
                ReSolve();
                buffer.Length = 0;
                foreach (var s in inventionDescriptions)
                {
                    buffer.AppendLine(s);
                }

                Driver.CommandResponse = buffer.ToString();
            }
            Input = "";
        }
        catch (GrammaticalError ex)
        {
            Driver.CommandResponse = $"{ex.Message}\n";
            var firstOne = true;
            foreach (var r in Syntax.RulesMatchingKeywords(Parser.Input))
            {
                if (firstOne)
                {
                    Driver.CommandResponse += "Perhaps you meant one of these sentence patterns:\n\n";
                    firstOne = false;
                }
                Driver.CommandResponse += $"{r.HelpDescription}";
            }
            LogFile.Log(ex.Message);
            LogFile.Log(ex.StackTrace);
        }

        Output = Driver.CommandResponse;
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
                //Graph.Create();  // Remove existing graph, if any
                inventionDescriptions = new[] {"Can't think of any - maybe there's a contradiction?"};
            }
            else
            {
                inventionDescriptions = new string[Generator.Current.Individuals.Count];
                for (var i = 0; i < Generator.Current.Individuals.Count; i++)
                    inventionDescriptions[i] =
                        invention.Description(Generator.Current.Individuals[i], "<b><color=grey>", "</color></b>");
                //MakeGraph();
            }
        }
    }
}
