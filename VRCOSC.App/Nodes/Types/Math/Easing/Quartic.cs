// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Quartic In", "Math/Easing")]
public sealed class QuarticInNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Utils.Easing.Quartic.In(float.Clamp(x, 0f, 1f));
    }
}

[Node("Quartic Out", "Math/Easing")]
public sealed class QuarticOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Utils.Easing.Quartic.Out(float.Clamp(x, 0f, 1f));
    }
}

[Node("Quartic InOut", "Math/Easing")]
public sealed class QuarticInOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Utils.Easing.Quartic.InOut(float.Clamp(x, 0f, 1f));
    }
}