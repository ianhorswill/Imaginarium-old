﻿#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationFiles.cs" company="Ian Horswill">
// Copyright (C) 2019 Ian Horswill
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

using System.IO;
using System.Security;
using UnityEngine;

/// <summary>
/// Provides methods for finding definitions and other configuration files.
/// </summary>
public static class ConfigurationFiles
{
    public static readonly string ProjectsDirectory = Path.Combine(Application.dataPath, "Projects");

    public static string ProjectPath(string projectName) => Path.Combine(ProjectsDirectory, projectName);

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

    public const string SourceExtension = ".gen";

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
