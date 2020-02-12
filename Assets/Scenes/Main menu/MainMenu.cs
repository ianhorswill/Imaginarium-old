using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void OnGUI()
    {
        Scenes.HandleSceneKeys();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
