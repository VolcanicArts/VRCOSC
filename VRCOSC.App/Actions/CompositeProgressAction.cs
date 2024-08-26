// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VRCOSC.App.Actions;

public class CompositeProgressAction : ProgressAction
{
    public override string Title => currentChild is CompositeProgressAction compositeCurrentChild ? compositeCurrentChild.Title : currentChild?.Title ?? string.Empty;

    private readonly List<ProgressAction> children = new();
    private ProgressAction? currentChild => children.FirstOrDefault(child => !child.IsComplete);

    public void AddAction(ProgressAction child)
    {
        children.Add(child);
    }

    protected override async Task Perform()
    {
        foreach (var child in children)
        {
            await child.Execute();
        }
    }

    public override float GetProgress()
    {
        var multiplier = 1f / children.Count;
        return children.Sum(child => child.IsComplete ? multiplier : map(child.GetProgress(), 0f, 1f, 0f, multiplier));
    }

    private static float map(float source, float sMin, float sMax, float dMin, float dMax) => dMin + (dMax - dMin) * ((source - sMin) / (sMax - sMin));
}
