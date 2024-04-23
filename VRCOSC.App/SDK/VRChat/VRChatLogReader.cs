// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VRCOSC.App.Modules;
using VRCOSC.App.SDK.Handlers;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.VRChat;

internal class VRChatLogReader
{
    private static readonly string logfile_location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"), "VRChat", "VRChat");
    private const string logfile_pattern = "output_log_*";
    private static readonly Regex world_exit_regex = new("^.+Fetching world information for (wrld_[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})$");
    private static readonly Regex world_enter_regex = new("^.+Finished entering world\\.$");

    private static readonly List<string> line_buffer = new();
    private static string? logFile;
    private static long byteOffset;
    private static Repeater processTask = null!;
    private static readonly object process_lock = new();

    public static string? CurrentWorldID { get; private set; }

    public static Action<string>? OnWorldEnter;

    internal static void Start()
    {
        reset();

        processTask = new Repeater(process);
        processTask.Start(TimeSpan.FromMilliseconds(200));
    }

    internal static async void Stop()
    {
        await processTask.StopAsync();

        reset();
    }

    private static void reset()
    {
        line_buffer.Clear();
        logFile = null;
        byteOffset = 0;
        CurrentWorldID = null;
    }

    private static void process()
    {
        lock (process_lock)
        {
            readLinesFromFile();
            if (!line_buffer.Any()) return;

            checkWorldExit();
            checkWorldEnter();

            line_buffer.Clear();
        }
    }

    private static void readLinesFromFile()
    {
        try
        {
            var localLogFile = Directory.GetFiles(logfile_location, logfile_pattern).MaxBy(d => new FileInfo(d).CreationTime);

            if (localLogFile != logFile)
            {
                reset();
                logFile = localLogFile;
                Logger.Log($"Reading log file: {logFile}");
            }

            if (logFile is null) return;

            using var fileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(fileStream);

            streamReader.BaseStream.Seek(byteOffset, SeekOrigin.Begin);

            var linesRead = 0;

            while (linesRead < 100 && streamReader.ReadLine() is { } line)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    line_buffer.Add(line);

                byteOffset += Encoding.UTF8.GetBytes(line).Length;
                linesRead++;
            }
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, "Could not read partial lines from log file");
        }
    }

    private static void checkWorldExit()
    {
        string? newWorldID = null;

        foreach (var line in line_buffer)
        {
            var worldIDGroup = world_exit_regex.Matches(line).LastOrDefault()?.Groups.Values.LastOrDefault();
            if (worldIDGroup is null) continue;

            var worldIDCapture = worldIDGroup.Captures.FirstOrDefault();
            if (worldIDCapture is null) continue;

            newWorldID = worldIDCapture.Value;
        }

        if (newWorldID is not null && newWorldID != CurrentWorldID)
        {
            CurrentWorldID = newWorldID;
            Logger.Log("Detected world leave");

            ModuleManager.GetInstance().GetModulesOfType<IVRCClientEventHandler>().ForEach(handler => handler.OnWorldExit());
        }
    }

    private static void checkWorldEnter()
    {
        foreach (var line in line_buffer.AsEnumerable().Reverse())
        {
            if (world_enter_regex.IsMatch(line))
            {
                Logger.Log($"Detected world enter to '{CurrentWorldID}'");
                OnWorldEnter?.Invoke(CurrentWorldID!);
                ModuleManager.GetInstance().GetModulesOfType<IVRCClientEventHandler>().ForEach(handler => handler.OnWorldEnter(CurrentWorldID!));
                break;
            }
        }
    }
}
