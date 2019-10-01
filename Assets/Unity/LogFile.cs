using System;
using System.IO;
using UnityEngine;

public static class LogFile
{
    private static string LogFilePath =>
        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + Path.DirectorySeparatorChar +
        "Imaginarium log.txt";

    private static TextWriter logFile;
    private static bool handlersInstalled;

    public static bool Enabled
    {
        get => logFile != null;
        set
        {
            if (value != Enabled)
            {
                if (value)
                {
                    logFile = File.CreateText(LogFilePath);
                    logFile.WriteLine($"Debugging log created {DateTime.Now}");
                    logFile.WriteLine();
                    separated = true;
                    if (!handlersInstalled)
                    {
                        Application.quitting += () =>
                        {
                            logFile?.Close();
                            logFile = null;
                        };
                        Application.logMessageReceived += (condition, trace, type) =>
                        {
                            if (logFile != null)
                            {
                                logFile.WriteLine($"{type}: {condition}");
                                logFile.WriteLine(trace);
                            }
                        };
                        handlersInstalled = true;
                    }
                }
                else
                {
                    logFile.Close();
                    logFile = null;
                }
            }
        }
    }

    private static bool separated;

    public static void Log(string message)
    {
        logFile?.WriteLine(message);
        separated = false;
    }

    public static void Log(string format, params object[] args)
    {
        logFile?.WriteLine(format, args);
        separated = false;
    }

    public static void Separate()
    {
        if (!separated)
        {
            logFile?.WriteLine();
            separated = true;
        }
    }
}
