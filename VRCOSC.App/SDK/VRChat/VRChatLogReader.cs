// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.VRChat;

internal class VRChatLogReader
{
    private static readonly string logfile_location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"), "VRChat", "VRChat");
    private const string logfile_pattern = "output_log_*";
    private static readonly Regex world_regex = new("^.+Fetching world information for (wrld_[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})$");

    private static readonly List<string> line_buffer = new();
    private static string? logFile;
    private static int lineNumber;
    private static Repeater processTask = null!;
    private static readonly object process_lock = new();

    public static string? CurrentWorldID { get; private set; }

    public static Action? OnWorldExit;
    public static Action<string>? OnWorldEnter;

    internal static void Init()
    {
        processTask = new Repeater(process);
        processTask.Start(TimeSpan.FromMilliseconds(200));
    }

    public static void Reset()
    {
        line_buffer.Clear();
        logFile = null;
        lineNumber = 0;
        CurrentWorldID = null;
    }

    private static void process()
    {
        lock (process_lock)
        {
            line_buffer.Clear();

            readLinesFromFile();
            if (!line_buffer.Any()) return;

            var newCurrentWorldID = findWorldExitEvent();

            if (newCurrentWorldID is not null && newCurrentWorldID != CurrentWorldID)
            {
                CurrentWorldID = newCurrentWorldID;
                Logger.Log($"Detected current world change to '{CurrentWorldID}'");

                OnWorldExit?.Invoke();
            }
        }
    }

    private static void readLinesFromFile()
    {
        try
        {
            var localLogFile = Directory.GetFiles(logfile_location, logfile_pattern).MaxBy(d => new FileInfo(d).CreationTime);

            if (localLogFile != logFile)
            {
                logFile = localLogFile;
                lineNumber = 0;
                Logger.Log($"Reading log file: {logFile}");
            }

            if (logFile is null) return;

            using var fileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(fileStream);
            streamReader.BaseStream.Seek(0, SeekOrigin.Begin);

            for (var i = 0; i < lineNumber; i++)
            {
                if (streamReader.ReadLine() == null) return;
            }

            while (streamReader.ReadLine() is { } line)
            {
                line_buffer.Add(line);
                lineNumber++;
            }
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, "Could not read partial lines from log file");
        }
    }

    private static string? findWorldExitEvent()
    {
        foreach (var line in line_buffer)
        {
            var latestWorld = world_regex.Matches(line).LastOrDefault()?.Groups.Values.LastOrDefault();
            if (latestWorld is null) continue;

            var latestWorldId = latestWorld.Captures.FirstOrDefault();
            if (latestWorldId is null) continue;

            return latestWorldId.Value;
        }

        return null;
    }
}
