using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

/// <summary>
/// Class with the methods to run scripts in the command line.
/// </summary>
public static class AudioDeviceManagerCommands
{
    private static string BasePath => Application.streamingAssetsPath + "/AudioDeviceManager";

    private static string ExecutablePath
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    // Check if the build is 64bit or 32bit.
                    return IntPtr.Size == 8
                        ? BasePath + "/win64/SoundVolumeView.exe"
                        : BasePath + "/win32/SoundVolumeView.exe";
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return BasePath + "/macOS/SwitchAudioSource";
                default:
                    UnityEngine.Debug.LogError("AudioDeviceManager is only available for Windows and macOS!");
                    return null;
            }
        }
    }

    /// <summary>
    /// Executes a command in the command line.
    /// </summary>
    /// <param name="arguments">Command arguments</param>
    /// <param name="standardOutputAction">Action to deal with the command output (optional)</param>
    public static void ExecuteCommand(string arguments, Action<List<string>> standardOutputAction = null)
    {
        Command(ExecutablePath, arguments, standardOutputAction);
    }

    /// <summary>
    /// Creates a thread to execute a command in the command line.
    /// </summary>
    /// <param name="arguments">Command arguments</param>
    /// <param name="standardOutputAction">Action to deal with the command output (optional)</param>
    public static void ExecuteAsyncCommand(string arguments, Action<List<string>> standardOutputAction = null)
    {
        string executablePath = ExecutablePath;
        Thread thread = new Thread(delegate () { Command(executablePath, arguments, standardOutputAction); });
        thread.Start();
    }

    /// <summary>
    /// Executes a command in the command line.
    /// </summary>
    /// <param name="executablePath">Path of the executable</param>
    /// <param name="arguments">Command arguments</param>
    /// <param name="standardOutputAction">Action to deal with the command output (nullable)</param>
    private static void Command(string executablePath, string arguments, Action<List<string>> standardOutputAction)
    {
        ProcessStartInfo processInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments = arguments,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = standardOutputAction != null
        };

        Process process = Process.Start(processInfo);

        if (standardOutputAction != null)
        {
            List<string> lines = new List<string>();
            while (process.StandardOutput.EndOfStream == false)
            {
                lines.Add(process.StandardOutput.ReadLine());
            }
            standardOutputAction(lines);
        }

        process.WaitForExit();
        process.Close();
    }
}
