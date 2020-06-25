﻿#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RepoManager.cs" company="Ian Horswill">
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
using System.IO;
using System.Linq;
using Imaginarium.Driver;
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
    public TMPro.TextMeshProUGUI ServerResponse;

    private string RepoToCloneUrl => RepoUrlField.text;

    // Start is called before the first frame update
    // ReSharper disable once UnusedMember.Local
    void Start()
    {
        Populate();
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
        if (!Directory.Exists(dir) || !Directory.Exists(dir+ Path.DirectorySeparatorChar+".git"))
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
        if (pName.StartsWith("https://") || pName.StartsWith("http://"))
            Import(pName);
        else ServerResponse.text = "Please enter a valid URL for a git repo.";
    }

    private void Import(string url)
    {
        bool success = true;
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
            //Directory.CreateDirectory(localPath);
            try
            {
                var response = Repository.Clone(url + ".git", localPath);
                if (!response.StartsWith(localPath))
                {
                    ServerResponse.text = "Couldn't add repo: " + response;
                    success = false;
                }
            }
            catch (Exception e)
            {
                success = false;
                ServerResponse.text = $"{e.GetType().Name}: {e.Message}";
            }
        }

        if (success)
            // Reload current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Pull(string localPath)
    {
        var succeeded = false;
        try
        {
            using (var repo = new Repository(localPath))
            {

                var remote = repo.Network.Remotes["origin"];
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);

                Commands.Fetch(repo, remote.Name, refSpecs, null, "");
                Commands.Checkout(repo, repo.Branches["master"],
                    new CheckoutOptions() {CheckoutModifiers = CheckoutModifiers.Force});
                succeeded = true;
            }

        }
        catch (Exception e)
        {
            ServerResponse.text = e.Message;
        }

        if (succeeded)
            ServerResponse.text = "Repo updated!";
    }

    // Currently non-functional because the current libgit2sharp leaves a symbolic
    // link in the repository and the .NET libraries give no way to detect or
    // delete a symbolic link.
    private void DeleteRepo(string localPath)
    {
        foreach (var d in Directory.GetDirectories(localPath))
        {
            var fileName = Path.GetFileName(d); 
            if (fileName != null && fileName.StartsWith("_git2_"))
                File.Delete(d);
        }

        Directory.Delete(localPath, true);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
