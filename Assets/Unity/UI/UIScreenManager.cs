#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UIScreenManager.cs" company="Ian Horswill">
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

using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// Switches between different UI screens
/// </summary>
// ReSharper disable once InconsistentNaming
public class UIScreenManager : MonoBehaviour
{
    public string InitialScreen;

    private static UIScreenManager singleton;

    private static IEnumerable<GameObject> Screens()
    {
        var singletonTransform = singleton.transform;
        for (int i = 0; i < singletonTransform.childCount; i++)
        {
            yield return singletonTransform.GetChild(i).gameObject;
        }
    }

    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        QualitySettings.vSyncCount = 1;
        singleton = this;
        SetScreen(InitialScreen);
    }

    // ReSharper disable once UnusedMember.Local
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            NextScreen();
        #if UNITY_EDITOR
        // The editor ignores the vertical sync in quality settings.  So this is the
        // only way to throttle CPU usage when the game is idle.
        Thread.Sleep(10);
        #endif
    }

    // ReSharper disable once UnusedMember.Local
    void OnGUI()
    {
        Scenes.HandleSceneKeys(Scenes.Menu);
    }

    private int currentScreen;
    public void NextScreen()
    {
        currentScreen = (currentScreen + 1) % transform.childCount;
        SetScreen(transform.GetChild(currentScreen).name);
    }

    /// <summary>
    /// Switches to the UI screen with the specified name
    /// </summary>
    /// <param name="screenName">Name of the UI screen to switch to.  This should be the name of a child object of the top-level canvas.</param>
    public static void SetScreen(string screenName)
    {
        foreach (var mode in Screens())
            mode.SetActive(mode.name == screenName);
    }

    public void SwitchTo(string screenName)
    {
        SetScreen(screenName);
    }
}

