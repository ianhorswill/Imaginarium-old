using System;
using System.IO;
using System.Security;
using UnityEngine;

/// <summary>
/// Provides methods for finding definitions and other configuration files.
/// </summary>
public static class ConfigurationFiles
{
    // This has to be outside the ApplicationHome method, or SecurityException occurs before the try.
    private static string UnityPath => Application.dataPath;

    /// <summary>
    /// Path to the directory containing configuration files.
    /// </summary>
    public static string ApplicationHome
    {
        get
        {
            try { return UnityPath; }
            catch (SecurityException)
            {
                // We're running in a test context, not inside of Unity
                return "../../../Assets";
            }
        }
    }

    /// <summary>
    /// Path the the configuration directory with the specified name
    /// </summary>
    public static string ConfigurationDirectory(string directoryName) => 
        Path.Combine(ApplicationHome, directoryName);

    /// <summary>
    /// Path to the specified configuration file
    /// </summary>
    public static string PathTo(string directoryName, string fileName, string extension = ".txt")
    {
        if (!Path.HasExtension(fileName))
            fileName += extension;
        return Path.Combine(ConfigurationDirectory(directoryName), fileName);
    }

}
