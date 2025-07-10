// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.App.Utils;

public class Repeater
{
    private readonly string name;
    private readonly Action? action;
    private readonly Func<Task>? actionTask;
    private CancellationTokenSource? cancellationTokenSource;
    private Task? updateTask;

    public Repeater(string name, Action action)
    {
        this.name = name;
        this.action = action;
    }

    public Repeater(string name, Func<Task> actionTask)
    {
        this.name = name;
        this.actionTask = actionTask;
    }

    public void Start(TimeSpan interval, bool runOnceImmediately = false)
    {
        if (cancellationTokenSource is not null && updateTask is not null) throw new InvalidOperationException($"{nameof(Repeater)}:{name} is already started");

        try
        {
            cancellationTokenSource = new CancellationTokenSource();
            updateTask = Task.Run(() => update(interval, runOnceImmediately), cancellationTokenSource.Token);
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{nameof(Repeater)}:{name} has experienced an exception");
        }
    }

    private async Task update(TimeSpan interval, bool runOnceImmediately)
    {
        Debug.Assert(cancellationTokenSource is not null);

        if (runOnceImmediately)
        {
            try
            {
                action?.Invoke();
                await (actionTask?.Invoke() ?? Task.CompletedTask);
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e, $"{nameof(Repeater)}:{name} has experienced an exception");
            }
        }

        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(interval, cancellationTokenSource.Token);
                action?.Invoke();
                await (actionTask?.Invoke() ?? Task.CompletedTask);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e, $"{nameof(Repeater)}:{name} has experienced an exception");
            }
        }
    }

    public async Task StopAsync()
    {
        if (cancellationTokenSource is null || updateTask is null)
            return;

        await cancellationTokenSource.CancelAsync();

        try
        {
            await updateTask;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{nameof(Repeater)}:{name} has experienced an exception");
        }
        finally
        {
            updateTask.Dispose();
            updateTask = null;

            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
    }
}