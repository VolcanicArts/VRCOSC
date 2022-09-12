// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.Util;

public sealed class TimedTask
{
    private readonly Action action;
    private readonly double deltaTimeMilli;
    private readonly bool executeOnceImmediately;

    private PeriodicTimer? timer;
    private Task? timerTask;

    public TimedTask(Action action, double deltaTimeMilli, bool executeOnceImmediately = false)
    {
        this.action = action;
        this.deltaTimeMilli = deltaTimeMilli;
        this.executeOnceImmediately = executeOnceImmediately;
    }

    public TimedTask Start()
    {
        timer = new PeriodicTimer(TimeSpan.FromMilliseconds(deltaTimeMilli));
        timerTask = executeWork();
        return this;
    }

    private async Task executeWork()
    {
        if (executeOnceImmediately) action.Invoke();

        while (await timer!.WaitForNextTickAsync())
        {
            action.Invoke();
        }
    }

    public async Task Stop()
    {
        if (timerTask is null) return;

        // timer allows us to dispose to cancel
        timer!.Dispose();
        await timerTask;
    }
}
