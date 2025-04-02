// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VRCOSC.App.Modules;
using VRCOSC.App.SDK.Handlers;
using VRCOSC.App.Utils;

// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable MissingBlankLines

namespace VRCOSC.App.SDK.VRChat;

internal static class VRChatLogReader
{
    private static readonly string logfile_location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"), "VRChat", "VRChat");
    private const string logfile_pattern = "output_log_*";

    private static readonly Regex datetime_regex = new(@"^(\d{4}\.\d{2}\.\d{2} \d{2}:\d{2}:\d{2}).+$");
    private static readonly Regex instance_left_regex = new("^.+OnLeftRoom$");
    private static readonly Regex instance_change_regex = new("^.+Destination set: (.+):.+$");
    private static readonly Regex instance_joined_regex = new(@"^.+Finished entering world\.$");
    private static readonly Regex user_joined_regex = new(@"^.+OnPlayerJoined .+ \((.+)\)$");
    private static readonly Regex user_left_regex = new(@"^.+OnPlayerLeft .+ \((.+)\)$");
    private static readonly Regex avatar_prechange_regex = new(@"^.+Initialize Limb Avatar VRCPlayer\[Local\] 1 True 1$");

    private static readonly List<LogLine> line_buffer = [];
    private static string? logFile;
    private static long byteOffset;
    private static Repeater? processTask;
    private static readonly object process_lock = new();

    private static string? currentWorldId { get; set; }

    public static Action<string>? OnWorldEnter;

    internal static void Start()
    {
        reset();

        if (!Directory.Exists(logfile_location))
        {
            Logger.Log("Cancelling log scanning. Cannot find the default VRChat directory");
            return;
        }

        processTask = new Repeater($"{nameof(VRChatLogReader)}-{nameof(process)}", process);
        processTask.Start(TimeSpan.FromMilliseconds(50), true);
    }

    internal static async Task Stop()
    {
        if (processTask is null) return;

        await processTask.StopAsync();
        reset();
    }

    private static void reset()
    {
        line_buffer.Clear();
        logFile = null;
        byteOffset = 0;
        currentWorldId = null;
    }

    private static void process()
    {
        lock (process_lock)
        {
            readLinesFromFile();
            if (line_buffer.Count == 0) return;

            foreach (var logLine in line_buffer)
            {
                checkInstanceLeft(logLine);
                checkInstanceChange(logLine);
                checkInstanceJoined(logLine);
                checkUserLeft(logLine);
                checkUserJoined(logLine);
                checkAvatarPreChange(logLine);
            }

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
                var dateTime = parseDate(line);

                if (!string.IsNullOrWhiteSpace(line) && dateTime is not null)
                {
                    line_buffer.Add(new LogLine(dateTime.Value, line));
                    linesRead++;
                }

                byteOffset += Encoding.UTF8.GetBytes(line).Length;
            }
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, "Could not read partial lines from log file");
        }
    }

    private static DateTime? parseDate(string line)
    {
        var foundDateTime = datetime_regex.Matches(line).LastOrDefault()?.Groups.Values.LastOrDefault()?.Value;
        if (foundDateTime is null) return null;

        return DateTime.ParseExact(foundDateTime, "yyyy.MM.dd HH:mm:ss", null);
    }

    private static IEnumerable<IVRCClientEventHandler> handlers => ModuleManager.GetInstance().GetRunningModulesOfType<IVRCClientEventHandler>();

    private static void checkInstanceLeft(LogLine logLine)
    {
        var match = instance_left_regex.Match(logLine.Line);
        if (!match.Success) return;

        handlers.ForEach(handler => handler.OnInstanceLeft(new VRChatClientEventInstanceLeft(logLine.DateTime)));
    }

    private static void checkInstanceChange(LogLine logLine)
    {
        var match = instance_change_regex.Match(logLine.Line);
        if (!match.Success) return;

        currentWorldId = match.Groups[1].Captures[0].Value;
    }

    private static void checkInstanceJoined(LogLine logLine)
    {
        var match = instance_joined_regex.Match(logLine.Line);
        if (!match.Success) return;

        if (currentWorldId is null)
        {
            Logger.Log("Entered world without knowing the world Id");
            return;
        }

        OnWorldEnter?.Invoke(currentWorldId);
        handlers.ForEach(handler => handler.OnInstanceJoined(new VRChatClientEventInstanceJoined(logLine.DateTime, currentWorldId)));
    }

    private static void checkUserLeft(LogLine logLine)
    {
        var match = user_left_regex.Match(logLine.Line);
        if (!match.Success) return;

        var userId = match.Groups[1].Captures[0].Value;
        handlers.ForEach(handler => handler.OnUserLeft(new VRChatClientEventUserLeft(logLine.DateTime, userId)));
    }

    private static void checkUserJoined(LogLine logLine)
    {
        var match = user_joined_regex.Match(logLine.Line);
        if (!match.Success) return;

        var userId = match.Groups[1].Captures[0].Value;
        handlers.ForEach(handler => handler.OnUserJoined(new VRChatClientEventUserJoined(logLine.DateTime, userId)));
    }

    private static void checkAvatarPreChange(LogLine logLine)
    {
        var match = avatar_prechange_regex.Match(logLine.Line);
        if (!match.Success) return;

        handlers.ForEach(handler => handler.OnAvatarPreChange(new VRChatClientEventAvatarPreChange(logLine.DateTime)));
    }

    private readonly record struct LogLine(DateTime DateTime, string Line);
}

public record VRChatClientEvent(DateTime DateTime);

public record VRChatClientEventInstanceLeft(DateTime DateTime) : VRChatClientEvent(DateTime);

public record VRChatClientEventInstanceJoined(DateTime DateTime, string WorldId) : VRChatClientEvent(DateTime);

public record VRChatClientEventUserLeft(DateTime DateTime, string UserId) : VRChatClientEvent(DateTime);

public record VRChatClientEventUserJoined(DateTime DateTime, string UserId) : VRChatClientEvent(DateTime);

public record VRChatClientEventAvatarPreChange(DateTime DateTime) : VRChatClientEvent(DateTime);