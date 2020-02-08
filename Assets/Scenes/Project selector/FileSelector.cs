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

    // Start is called before the first frame update
    // ReSharper disable once UnusedMember.Local
    void Start()
    {
        Populate(Path.Combine(Application.dataPath, "Projects"));
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
        if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
            LeaveScene();
    }

    private void Select(string dir)
    {
        Parser.DefinitionsDirectory = dir;
        LeaveScene();
    }

    private void LeaveScene()
    {
        SceneManager.LoadSceneAsync(NextScene);
    }
}
