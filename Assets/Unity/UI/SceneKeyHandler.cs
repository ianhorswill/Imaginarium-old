using UnityEngine;

/// <summary>
/// Handles global key bindings for screen switching (ESC and HELP)
/// </summary>
public class SceneKeyHandler : MonoBehaviour
{
    public void OnGUI()
    {
        Scenes.HandleSceneKeys();
    }
}
