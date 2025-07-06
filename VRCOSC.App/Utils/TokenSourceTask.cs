// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace VRCOSC.App.Utils;

/// <summary>
/// Pairs a <see cref="CancellationTokenSource"/> and <see cref="Task"/>
/// </summary>
public record TokenSourceTask(CancellationTokenSource Source, Task Task)
{
    public async Task CancelAndWaitAsync()
    {
        await Source.CancelAsync();

        try
        {
            await Task.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }
}