using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;

// Output the build size or a failure depending on BuildPlayer.

public class BuildPlayer
    : MonoBehaviour
{
    [MenuItem("Build/Build all")]
    public static void BuildAll()
    {
        Build(BuildTarget.StandaloneWindows64, "Builds/Imaginarium windows/Imaginarium.exe");
        Build(BuildTarget.StandaloneOSX, "Builds/Imaginarium OSX.app");
        //Build(BuildTarget.StandaloneLinux64, "Builds/Linux");
    }

    public static void Build(BuildTarget target, string locationPath)
    {
        Debug.Log($"Building {target} to {locationPath}");

        var options = new BuildPlayerOptions
        {
            scenes = new[]
            {
                "Assets/Scenes/Help.unity",
                "Assets/Scenes/Repl/Repl.unity",
                "Assets/Scenes/Server.unity",
                "Assets/Scenes/Main menu/Main menu.unity",
                "Assets/Scenes/Project selector/Project selector.unity",
                "Assets/Scenes/Repo manager/Repo manager.unity"
            },
            locationPathName = locationPath,
            target = target,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"{target} build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log($"Build {target} failed");
        }
    }
}