// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.App.Utils;

/// <summary>
/// Pairs a <see cref="CancellationTokenSource"/> and <see cref="Task"/>
/// </summary>
public record TokenSourceTask
{
    public CancellationTokenSource Source { get; }
    public Task? Task { get; private set; }

    public TokenSourceTask(CancellationTokenSource source, Task task)
    {
        Source = source;
        Task = task;
    }

    public TokenSourceTask(CancellationTokenSource source)
    {
        Source = source;
    }

    public void SetTask(Task task) => Task = task;

    public async Task CancelAndWaitAsync()
    {
        await Source.CancelAsync();

        if (Task is null) return;

        try
        {
            await Task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }
}