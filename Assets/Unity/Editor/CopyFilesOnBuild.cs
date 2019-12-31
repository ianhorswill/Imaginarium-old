using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Callbacks;

/// <summary>
/// Copy the definition files, etc. into the builds
/// </summary>
public class CopyFilesOnBuild
{
    private static readonly string[] DataDirectories = {"Inflections", "Definitions"};
    [PostProcessBuild()]
    // ReSharper disable once IdentifierTypo
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        var buildDirectory = Path.GetDirectoryName(pathToBuiltProject);
        if (buildDirectory == null)
            throw new FileNotFoundException("Invalid build directory", pathToBuiltProject);
        string dataDirectory;

        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneLinux64:
                dataDirectory = Path.ChangeExtension(pathToBuiltProject, null) + "_data";
                break;

            case BuildTarget.StandaloneOSX:
                dataDirectory = pathToBuiltProject + "/Contents";
                break;

            default:
                throw new ArgumentException("Don't know how to build Prolog code for target: "+target);
        }

        void CopyDirectory(string directoryName)
        {
            CopyTree(
                ConfigurationFiles.ConfigurationDirectory(directoryName),
                Path.Combine(dataDirectory, directoryName)
                );
        }

        // Recursively copy subtree from to subtree to.
        void CopyTree(string from, string to)
        {
            //if (Directory.Exists(to))
            //    Directory.Delete(to, true);
            Directory.CreateDirectory(to);

            foreach (var d in Directory.GetDirectories(from))
            {
                var name = Path.GetFileName(d);
                CopyTree(
                    Path.Combine(from, name),
                    Path.Combine(to, name)
                    );
            }

            foreach (var f in Directory.GetFiles(from))
            {
                // Ignore internal files of emacs and Unity
                if (f.EndsWith("~") || f.EndsWith(".meta"))
                    continue;
                var name = Path.GetFileName(f);
                if (name.StartsWith("#"))
                    break;  // emacs temp file
                File.Copy(
                    Path.Combine(from, name),
                    Path.Combine(to, name)
                    );
            }
        }

        foreach (var d in DataDirectories)
            CopyDirectory(d);
    }
}
