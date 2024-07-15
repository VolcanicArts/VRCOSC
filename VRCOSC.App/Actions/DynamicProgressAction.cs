// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Actions;

public class DynamicProgressAction : ProgressAction
{
    public override string Title { get; }
    private readonly Action callback;

    public DynamicProgressAction(string title, Action callback)
    {
        Title = title;
        this.callback = callback;
    }

    protected override Task Perform()
    {
        callback();
        return Task.CompletedTask;
    }

    public override float GetProgress() => 0f;
}

public class DynamicAsyncProgressAction : ProgressAction
{
    public override string Title { get; }
    private readonly Func<Task> callback;

    public DynamicAsyncProgressAction(string title, Func<Task> callback)
    {
        Title = title;
        this.callback = callback;
    }

    protected override async Task Perform() => await callback();

    public override float GetProgress() => 0f;
}
