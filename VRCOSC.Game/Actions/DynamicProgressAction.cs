// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.Actions;

public class DynamicProgressAction : ProgressAction
{
    private readonly string title;
    private readonly Action callback;

    public override string Title => title;

    public DynamicProgressAction(string title, Action callback)
    {
        this.title = title;
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
    private readonly string title;
    private readonly Func<Task> callback;

    public override string Title => title;

    public DynamicAsyncProgressAction(string title, Func<Task> callback)
    {
        this.title = title;
        this.callback = callback;
    }

    protected override async Task Perform() => await callback();

    public override float GetProgress() => 0f;
}
