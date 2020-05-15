using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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