﻿#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileSelector.cs" company="Ian Horswill">
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

using System.IO;
using System.Linq;
using LibGit2Sharp;
using UnityEngine;
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
        get => new string(NewProjectNameField.text.Where(c => (int) c < 128).ToArray());
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
        Populate();
        NewProjectName = NewProjectNamePrompt;
    }

    private void Populate()
    {
        Content.DestroyAllChildren();

        foreach (var dir in ConfigurationFiles.SearchPath)
            Populate(dir);
    }

    void Populate(string parentPath)
    {
        if (!Directory.Exists(parentPath))
            return;

        foreach (var dir in Directory.GetDirectories(parentPath))
        {
            var fileName = Path.GetFileName(dir);
            if (fileName == null || fileName.StartsWith(".git") || fileName.StartsWith("_git"))
                continue;

            var button = Instantiate(ButtonPrefab, Content);
            button.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = fileName;
            button.GetComponent<Button>().onClick.AddListener(() => Select(dir));
        }
    }

    public void OnGUI()
    {
        Scenes.HandleSceneKeys();
    }

    private void Select(string dir)
    {
        PlayerPrefs.SetString("DefinitionsDirectory", dir);
        PlayerPrefs.Save();
        UIDriver.Ontology.DefinitionsDirectory = dir;
        LeaveScene();
    }

    private void LeaveScene()
    {
        Scenes.SwitchToRepl();
    }

    public void CreateProject()
    {
        var pName = NewProjectName.Trim();

        if (pName.StartsWith("https://") || pName.StartsWith("http://"))
        {
            Import(pName);
            NewProjectName = "";
        }
        else
        {
            // Make a new project
            if (pName == NewProjectNamePrompt || pName == "")
                NewProjectName = NewProjectNameProd;

            else if (pName != NewProjectNameProd)
            {
                var path = ConfigurationFiles.ProjectPath(ConfigurationFiles.UserProjectsDirectory, pName);
                Directory.CreateDirectory(path);
                Select(path);
            }
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
}
