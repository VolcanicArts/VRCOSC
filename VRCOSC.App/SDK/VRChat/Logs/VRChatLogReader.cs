// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Handlers;
using VRCOSC.App.SDK.VRChat.Logs.Handlers;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.VRChat.Logs;

internal static class VRChatLogReader
{
    private static readonly string logfile_location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace("Local", "LocalLow"), "VRChat", "VRChat");
    private const string logfile_pattern = "output_log_*";

    private static readonly Regex datetime_regex = new(@"^(\d{4}\.\d{2}\.\d{2} \d{2}:\d{2}:\d{2}).+$");

    private static readonly List<LogLine> line_buffer = [];
    private static string? logFile;
    private static long byteOffset;
    private static Repeater? processTask;
    private static readonly Lock process_lock = new();

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
        state = new();
    }

    private static Task process()
    {
        lock (process_lock)
        {
            readLinesFromFile();
            if (line_buffer.Count == 0) return Task.CompletedTask;

            foreach (var logLine in line_buffer)
            {
                foreach (var logLineHandler in log_line_handlers)
                {
                    var match = logLineHandler.Regex.Match(logLine.Line);
                    if (!match.Success) continue;

                    var logEvent = logLineHandler.HandleMatch(state, new VRChatLogLineMatch(logLine.Timestamp, match));
                    if (logEvent is null) continue;

                    foreach (var eventHandler in event_handlers)
                    {
                        eventHandler.HandleClientEvent(logEvent);
                    }
                }
            }

            line_buffer.Clear();
        }

        return Task.CompletedTask;
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

    private readonly record struct LogLine(DateTime Timestamp, string Line);
}