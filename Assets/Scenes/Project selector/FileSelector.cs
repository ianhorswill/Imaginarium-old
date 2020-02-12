using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FileSelector : MonoBehaviour
{
    public Transform Content;
    public GameObject ButtonPrefab;
    [Tooltip("Scene file to switch to after project is selected")]
    public string NextScene;

    public TMPro.TextMeshProUGUI NewProjectNameField;

    private string NewProjectName
    {
        get => NewProjectNameField.text;
        set
        {
            NewProjectNameField.text = value;
            // If you only set it once, Unity will set the text field to a zero-width space (unicode 8203)
            // and nothing else.  Seriously.  Completely reproducible.
            NewProjectNameField.text = value;
        }
    }

    public string NewProjectNamePrompt = "Name for new generator";
    public string NewProjectNameProd = "Enter a name here first";

    // Start is called before the first frame update
    // ReSharper disable once UnusedMember.Local
    void Start()
    {
        Populate(ConfigurationFiles.ProjectsDirectory);
        NewProjectName = NewProjectNamePrompt;
    }

    void Populate(string parentPath)
    {
        foreach (var dir in Directory.GetDirectories(parentPath))
        {
            var button = Instantiate(ButtonPrefab, Content);
            button.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = Path.GetFileName(dir);
            button.GetComponent<Button>().onClick.AddListener(() => Select(dir));
        }
    }

    public void OnGUI()
    {
        Scenes.HandleSceneKeys();
    }

    private void Select(string dir)
    {
        Parser.DefinitionsDirectory = dir;
        LeaveScene();
    }

    private void LeaveScene()
    {
        Scenes.SwitchToRepl();
    }

    public void CreateProject()
    {
        var pName = NewProjectName.Trim();
        if (pName == NewProjectNamePrompt || pName == "")
            NewProjectName = NewProjectNameProd;
        else if (pName != NewProjectNameProd)
        {
            var path = ConfigurationFiles.ProjectPath(pName);
            Directory.CreateDirectory(path);
            Select(path);
        }
    }
}
