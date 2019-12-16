using System.Collections.Generic;
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
        singleton = this;
        SetScreen(InitialScreen);
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

