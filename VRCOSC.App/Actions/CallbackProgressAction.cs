// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Actions;

public class CallbackProgressAction : ProgressAction
{
    public override string Title { get; }
    private readonly Action? callback;
    private readonly Func<Task?>? taskCallback;

    public CallbackProgressAction(string title, Action callback)
    {
        Title = title;
        this.callback = callback;
    }

    public CallbackProgressAction(string title, Func<Task?> callback)
    {
        Title = title;
        taskCallback = callback;
    }

    protected override async Task Perform()
    {
        callback?.Invoke();
        await (taskCallback?.Invoke() ?? Task.CompletedTask);
    }
}