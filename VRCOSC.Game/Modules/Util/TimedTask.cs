// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.Util;

public class TimedTask : IDisposable
{
    private readonly Action action;
    private readonly PeriodicTimer timer;
    private readonly bool executeOnceImmediately;
    private CancellationTokenSource? cts;
    private Task? timerTask;

    public TimedTask(Action action, double deltaTimeMilli, bool executeOnceImmediately = false)
    {
        this.action = action;
        timer = new PeriodicTimer(TimeSpan.FromMilliseconds(deltaTimeMilli));
        this.executeOnceImmediately = executeOnceImmediately;
    }

    public void Start()
    {
        cts = new CancellationTokenSource();
        timerTask = executeWork();
    }

    private async Task executeWork()
    {
        if (executeOnceImmediately) action.Invoke();

        try
        {
            while (await timer.WaitForNextTickAsync(cts!.Token))
            {
                action.Invoke();
            }
        }
        catch (OperationCanceledException) { }
    }

    public async Task Stop()
    {
        if (timerTask == null) return;

        cts?.Cancel();
        await timerTask;
        timerTask.Dispose();
    }

    public void Dispose()
    {
        timer.Dispose();
        cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
