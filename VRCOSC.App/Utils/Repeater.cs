using System;
using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.App.Utils;

public sealed class Repeater : IAsyncDisposable
{
    private readonly string name;
    private readonly Func<Task> actionTask;

    private CancellationTokenSource? loopCts;
    private Task? loopTask;
    private readonly SemaphoreSlim gate = new(1, 1);

    public Repeater(
        string name,
        Func<Task> actionTask)
    {
        this.name = name ?? throw new ArgumentNullException(nameof(name));
        this.actionTask = actionTask ?? throw new ArgumentNullException(nameof(actionTask));
    }

    public void Start(TimeSpan interval, bool runOnceImmediately = false)
    {
        if (loopTask is not null)
            throw new InvalidOperationException($"{nameof(Repeater)} is already running.");

        loopCts = new CancellationTokenSource();
        loopTask = Task.Run(() => loopAsync(interval, runOnceImmediately, loopCts.Token), loopCts.Token);
    }

    public async Task StopAsync()
    {
        if (loopCts is null) return;

        await loopCts.CancelAsync();

        try
        {
            if (loopTask is not null)
                await loopTask;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            loopCts.Dispose();
            loopCts = null;
            loopTask = null;
        }
    }

    public async ValueTask DisposeAsync() => await StopAsync();

    private async Task loopAsync(TimeSpan interval, bool runOnceImmediately, CancellationToken loopToken)
    {
        using var timer = new PeriodicTimer(interval);

        if (runOnceImmediately)
            await SafelyInvokeOnceAsync();

        try
        {
            while (await timer.WaitForNextTickAsync(loopToken))
                await SafelyInvokeOnceAsync();
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task SafelyInvokeOnceAsync()
    {
        await gate.WaitAsync();

        try
        {
            try
            {
                await actionTask();
            }
            catch (OperationCanceledException)
            {
            }
        }
        catch (Exception ex)
        {
            ExceptionHandler.Handle(ex, $"{nameof(Repeater)}:{name} encountered an exception");
        }
        finally
        {
            gate.Release();
        }
    }
}