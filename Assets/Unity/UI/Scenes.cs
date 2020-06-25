#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Scenes.cs" company="Ian Horswill">
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

