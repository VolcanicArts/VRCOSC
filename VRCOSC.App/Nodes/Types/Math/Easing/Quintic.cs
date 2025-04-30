// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Quintic In", "Math/Easing")]
public sealed class QuinticInNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Utils.Easing.Quintic.In(float.Clamp(x, 0f, 1f));
    }
}

[Node("Quintic Out", "Math/Easing")]
public sealed class QuinticOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Utils.Easing.Quintic.Out(float.Clamp(x, 0f, 1f));
    }
}

[Node("Quintic InOut", "Math/Easing")]
public sealed class QuinticInOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Utils.Easing.Quintic.InOut(float.Clamp(x, 0f, 1f));
    }
}