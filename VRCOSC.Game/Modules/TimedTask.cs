// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.Game.Modules;

public sealed class TimedTask
{
    private readonly Func<Task> action;
    private readonly double deltaTimeMilli;
    private readonly bool executeOnceImmediately;

    private PeriodicTimer? timer;
    private Task? timerTask;

    public TimedTask(Func<Task> action, double deltaTimeMilli, bool executeOnceImmediately = false)
    {
        this.action = action;
        this.deltaTimeMilli = deltaTimeMilli;
        this.executeOnceImmediately = executeOnceImmediately;
    }

    public async Task Start()
    {
        timer?.Dispose();
        await (timerTask ?? Task.CompletedTask);

        timer = new PeriodicTimer(TimeSpan.FromMilliseconds(deltaTimeMilli));

        if (executeOnceImmediately) await action.Invoke();

        timerTask = Task.Run(executeWork);
    }

    private async void executeWork()
    {
        while (await timer!.WaitForNextTickAsync())
        {
            await action.Invoke();
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
