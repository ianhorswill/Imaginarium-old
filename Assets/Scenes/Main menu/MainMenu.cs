using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public TMPro.TextMeshProUGUI LogButtonText;

    public void Start()
    {
        UpdateToggleButton();
    }

    public void OnGUI()
    {
        Scenes.HandleSceneKeys();
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ToggleDebugLog()
    {
        LogFile.Enabled = !LogFile.Enabled;
        UpdateToggleButton();
    }

    private void UpdateToggleButton()
    {
        LogButtonText.text = LogFile.Enabled ? "End debug log" : "Start debug log";
    }
}
