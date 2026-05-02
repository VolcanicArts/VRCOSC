// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VRCOSC.App.SDK.Handlers;
using VRCOSC.App.SDK.VRChat.Logs.Handlers;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.VRChat.Logs;

internal static class VRChatLogReader
{
    private static readonly string logfile_location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace("Local", "LocalLow"), "VRChat", "VRChat");
    private const string logfile_pattern = "output_log_*";

    private static readonly Regex datetime_regex = new(@"^(\d{4}\.\d{2}\.\d{2} \d{2}:\d{2}:\d{2}).+$");

    private static string? logFile;
    private static long byteOffset;
    private static SpinWaitTask? processTask;

    private static readonly IVRChatLogLineHandler[] log_line_handlers =
    [
        new UserAuthenticatedLogLineHandler(),
        new RoomJoinedLogLineHandler(),
        new InstanceChangeLogLineHandler(),
        new InstanceJoinedLogLineHandler(),
        new InstanceLeftLogLineHandler(),
        new UserLeftLogLineHandler(),
        new UserJoinedLogLineHandler(),
        new AvatarChangeStartLogLineHandler()
    ];

    private static LogReaderState state = new();

    private static readonly List<IVRCClientEventHandler> event_handlers = [];

    internal static void Register(IVRCClientEventHandler handler) => event_handlers.Add(handler);
    internal static void DeRegister(IVRCClientEventHandler handler) => event_handlers.Remove(handler);

    internal static void Start()
    {
        reset();

        if (!Directory.Exists(logfile_location))
        {
            Logger.Log("Cancelling log scanning. Cannot find the default VRChat directory");
            return;
        }

        processTask = new SpinWaitTask(process);
        processTask.Start(TimeSpan.FromMilliseconds(10d));
    }

    internal static void Stop()
    {
        if (processTask is null) return;

        processTask.Stop();
        reset();
    }

    private static void reset()
    {
        logFile = null;
        byteOffset = 0;
        state = new();
    }

    private static void process()
    {
        try
        {
            readLinesToFileEnd();
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e);
        }
    }

    private static void handleLogLine(LogLine logLine)
    {
        foreach (var logLineHandler in log_line_handlers)
        {
            var match = logLineHandler.Regex.Match(logLine.Line);
            if (!match.Success) continue;

            var logEvent = logLineHandler.HandleMatch(state, new VRChatLogLineMatch(logLine.Timestamp, match));
            if (logEvent is null) continue;

            foreach (var eventHandler in event_handlers)
            {
                try
                {
                    eventHandler.HandleClientEvent(logEvent);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Handle(e, "Exception handling client event");
                }
            }
        }
    }

    private static void readLinesToFileEnd()
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

            if (!File.Exists(logFile)) return;

            try
            {
                using var fileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

                if (fileStream.Length < byteOffset)
                    byteOffset = 0;

                fileStream.Seek(byteOffset, SeekOrigin.Begin);

                using var reader = new StreamReader(fileStream, Encoding.UTF8);

                while (reader.ReadLine() is { } line)
                {
                    var dateTime = parseDate(line);
                    if (dateTime is null) continue;

                    handleLogLine(new LogLine(dateTime.Value, line));
                }

                byteOffset = fileStream.Position;
            }
            catch (IOException)
            {
                // Ignore if the file is locked
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

    private record LogLine(DateTime Timestamp, string Line);
}