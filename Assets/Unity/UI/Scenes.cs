using UnityEngine.SceneManagement;
using UnityEngine;

public static class Scenes
{
    public const string Repl = "Repl";
    public const string Menu = "Main menu";
    public const string Help = "Help";


    public static void SwitchTo(string name)
    {
        SceneManager.LoadSceneAsync(name);
    }

    public static void SwitchToRepl()
    {
        SwitchTo(Repl);
    }

    public static void SwitchToMenu()
    {
        SwitchTo(Menu);
    }

    public static void SwitchToHelp()
    {
        SwitchTo(Help);
    }

    public static void HandleSceneKeys(string escapeScene = Repl)
    {
        if (Event.current.isKey && Event.current.isKey && Input.GetKey(Event.current.keyCode))
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.Escape:
                    SwitchTo(escapeScene);
                    break;

                case KeyCode.Help:
                case KeyCode.F1:
                    SwitchToHelp();
                    break;
            }
        }
    }
}

