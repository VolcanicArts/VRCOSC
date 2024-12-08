// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Actions;

public class DynamicChildProgressAction : CompositeProgressAction
{
    private readonly Func<ProgressAction?> callback;

    public DynamicChildProgressAction(Func<ProgressAction?> callback)
    {
        this.callback = callback;
    }

    protected override async Task Perform()
    {
        var childAction = callback();
        AddAction(childAction);

        await base.Perform();
    }
}