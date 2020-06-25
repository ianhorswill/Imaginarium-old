#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SublimeTextConfigurator.cs" company="Ian Horswill">
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
using UnityEngine;

public class SublimeTextConfigurator : MonoBehaviour
{
    public const string SyntaxFile = "Imaginarium.sublime-syntax";

    /// <summary>
    /// Installs syntax highlighter for Sublime Text 3
    /// </summary>
    public void ConfigureSublime()
    {
        Debug.Log("called");
        var sublimeDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Sublime Text 3");
        if (Directory.Exists(sublimeDir))
        {
            Debug.Log("Found directory");
            var packagesDir = Path.Combine(sublimeDir, "Packages");
            if (!Directory.Exists(packagesDir))
                Directory.CreateDirectory(packagesDir);

            var userPackagesDir = Path.Combine(packagesDir, "User");
            if (!Directory.Exists(userPackagesDir))
                Directory.CreateDirectory(userPackagesDir);

            var toolsDir = ConfigurationFiles.ConfigurationDirectory("Tools");
            if (!Directory.Exists(toolsDir))
                Debug.Log("No directory!");
            File.Copy(
                Path.Combine(toolsDir, SyntaxFile),
                Path.Combine(userPackagesDir, SyntaxFile),
                true);
        }
    }
}