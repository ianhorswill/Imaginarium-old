using UnityEngine;

/// <summary>
/// Handles global key bindings for screen switching (ESC and HELP)
/// </summary>
public class SceneKeyHandler : MonoBehaviour
{
    public void Start()
    {
        QualitySettings.vSyncCount = 1;
    }

    public void OnGUI()
    {
        Scenes.HandleSceneKeys();
    }
}
