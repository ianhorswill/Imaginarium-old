using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIDriver : MonoBehaviour
{
    private Invention invention;
    private string[] inventionDescriptions;
    private readonly StringBuilder buffer = new StringBuilder();
    public string CommandResponse = "";
    public InputField InputField;
    public Text OutputField;
    public ScrollRect OutputScrollRect;
    public Scrollbar OutputVerticalScroll;
    public ContentSizeFitter OutputFitter;

    public void Start()
    {
        Parser.DefinitionsDirectory = Application.dataPath + "/Definitions";
        SelectInput();
    }

    public void OnEnable()
    {
        SelectInput();
    }

    private void SelectInput()
    {
        InputField.Select();
        InputField.ActivateInputField();
    }

    private string Input
    {
        get => InputField.text;
        set => InputField.text = value;
    }

    private string Output
    {
        set
        {
            OutputField.text = value;
            StartCoroutine(ScrollToTop());
        }
    }

    IEnumerator ScrollToTop()
    {
        yield return null;
        yield return null;
        OutputScrollRect.verticalNormalizedPosition = 1;
        yield return null;
        SelectInput();
    }

    private float ViewportSize => OutputScrollRect.GetComponent<RectTransform>().rect.height;
    private float OutputSize => OutputField.GetComponent<RectTransform>().rect.height;

    public void ScrollPages(int pages)
    {
        var pageSize = ViewportSize / OutputSize;
        OutputScrollRect.verticalNormalizedPosition = Mathf.Clamp(OutputScrollRect.verticalNormalizedPosition - pages*pageSize, 0, 1);
    }

    /// <summary>
    /// There must be a better way of doing this in the new UI
    /// </summary>
    void OnGUI()
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
        CommandResponse = "";

        try
        {
            if (Input != "")
                Parser.UserCommand(Input);
            if (Generator.Current != null)
            {
                Generator.Current.Rebuild();
                ReSolve();
                buffer.Length = 0;
                foreach (var s in inventionDescriptions)
                {
                    buffer.AppendLine(s);
                }

                CommandResponse = buffer.ToString();
            }
            Input = "";
        }
        catch (GrammaticalError ex)
        {
            CommandResponse = ex.Message;
            LogFile.Log(ex.Message);
            LogFile.Log(ex.StackTrace);
        }

        Output = CommandResponse;
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
