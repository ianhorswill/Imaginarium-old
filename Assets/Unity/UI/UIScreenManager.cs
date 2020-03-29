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

