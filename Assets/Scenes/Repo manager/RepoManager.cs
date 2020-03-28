using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using LibGit2Sharp;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RepoManager : MonoBehaviour
{
    public Transform Content;
    public GameObject ButtonPrefab;

    public TMPro.TextMeshProUGUI RepoUrlField;

    private string RepoToCloneUrl
    {
        get => RepoUrlField.text;
        set
        {
            RepoUrlField.text = value;
            // If you only set it once, Unity will set the text field to a zero-width space (unicode 8203)
            // and nothing else.  Seriously.  Completely reproducible.
            RepoUrlField.text = value;
        }
    }

    public string RepoUrlPrompt = "URL for repo to add";
    public string RepoUrlProd = "Enter a URL for a repo here first";

    // Start is called before the first frame update
    // ReSharper disable once UnusedMember.Local
    void Start()
    {
        Populate();
        RepoToCloneUrl = RepoUrlPrompt;
    }

    private void Populate()
    {
        Content.DestroyAllChildren();

        foreach (var dir in ConfigurationFiles.SearchPath)
            Populate(dir);
    }

    void Populate(string dir)
    {
        dir = new string(dir.Where(c => (int) c < 128).ToArray());
        if (!Directory.Exists(dir) || !Directory.Exists(dir+Path.DirectorySeparatorChar+".git"))
            return;

        var container = Instantiate(ButtonPrefab, Content);
        var repoName = container.transform.Find("Repo name");
        var textComponent = repoName.GetComponent<TMPro.TextMeshProUGUI>();
        textComponent.text = Path.GetFileName(dir);

        void InitButton(GameObject go, string buttonName, UnityAction handler)
        {
            var button = go.transform.Find(buttonName);

            if (button != null)
                button.GetComponent<Button>().onClick.AddListener(handler);
        }

        InitButton(container, "Delete", () => DeleteRepo(dir));
        InitButton(container, "Update", () => Pull(dir));
    }

    public void OnGUI()
    {
        Scenes.HandleSceneKeys();
    }

    public void CloneRepo()
    {
        var pName = RepoToCloneUrl.Trim();

        // Make a new project
        if (pName == RepoUrlPrompt || pName == "" || !(pName.StartsWith("https://") || pName.StartsWith("http://")))
            RepoToCloneUrl = RepoUrlProd;

        else if (pName != RepoUrlProd)
        {
            Import(pName);
            RepoToCloneUrl = "";
        }
    }

    private void Import(string url)
    {
        url = new string(url.Where(c => (int) c < 128).ToArray());
        var lastSlash = url.LastIndexOf('/');
        if (lastSlash < 0)
            return;
        var repoName = url.Substring(lastSlash+1, url.Length - (lastSlash + 1));
        var localPath = Path.Combine(ConfigurationFiles.UserReposDirectory, repoName);
        if (Directory.Exists(localPath))
            Pull(localPath);
        else
        {
            Directory.CreateDirectory(localPath);
            Repository.Clone(url + ".git", localPath);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static void Pull(string localPath)
    {
        using (var repo = new Repository(localPath))
        {
            var remote = repo.Network.Remotes["origin"];
            var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
            Commands.Fetch(repo, remote.Name, refSpecs, null, "");
            Commands.Checkout(repo, repo.Branches["master"],
                new CheckoutOptions() {CheckoutModifiers = CheckoutModifiers.Force});
        }
    }

    // Currently non-functional because the current libgit2sharp leaves a symbolic
    // link in the repository and the .NET libraries give no way to detect or
    // delete a symbolic link.
    private void DeleteRepo(string localPath)
    {
        foreach (var d in Directory.GetDirectories(localPath))
            if (Path.GetFileName(d).StartsWith("_git2_"))
                File.Delete(d);

        Directory.Delete(localPath, true);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
