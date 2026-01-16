// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PresentMonFps;

namespace VRCOSC.App.Utils;

public static class ProcessFPS
{
    private static readonly ConcurrentDictionary<int, ProcessFPSSession> sessions = [];

    public static double GetProcessFPS(Process process)
    {
        if (process is null) return 0;

        try
        {
            var processId = process.Id;

            if (process.HasExited)
            {
                cleanupProcessData(processId);
                return 0d;
            }

            if (!sessions.TryGetValue(processId, out var session))
            {
                session = new ProcessFPSSession((uint)processId);
                sessions[processId] = session;
                return 0d;
            }

            return session.FPS;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error getting FPS for process {process.Id}");
            return 0d;
        }
    }

    private static void cleanupProcessData(int processId)
    {
        if (!sessions.TryGetValue(processId, out var session)) return;

        session.Stop();
        sessions.TryRemove(processId, out _);
    }

    public static void DisposeAll()
    {
        foreach (var session in sessions.Values) session.Stop();

        sessions.Clear();

        try
        {
            if (FpsInspector.IsAvailable) FpsInspector.StopTraceSession();
        }
        catch
        {
        }
    }

    private class ProcessFPSSession
    {
        private readonly CancellationTokenSource cancellationSource;
        private readonly Task updateTask;

        public double FPS { get; private set; }

        public ProcessFPSSession(uint processId)
        {
            cancellationSource = new CancellationTokenSource();

            var request = new FpsRequest(processId)
            {
                PeriodMillisecond = 100
            };

            updateTask = FpsInspector.StartForeverAsync(request, result => FPS = result.Fps, cancellationSource.Token);
        }

        public void Stop()
        {
            try
            {
                cancellationSource.Cancel();
                updateTask.Wait();
            }
            catch
            {
            }
            finally
            {
                cancellationSource.Dispose();
            }
        }
    }
}