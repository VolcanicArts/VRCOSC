// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Math.Easing;

using VRCOSC.App.Utils;

[Node("Cubic In", "Math/Easing")]
public sealed class CubicInNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Easing.Cubic.In(float.Clamp(x, 0f, 1f));
    }
}

[Node("Cubic Out", "Math/Easing")]
public sealed class CubicOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Easing.Cubic.Out(float.Clamp(x, 0f, 1f));
    }
}

[Node("Cubic InOut", "Math/Easing")]
public sealed class CubicInOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Easing.Cubic.InOut(float.Clamp(x, 0f, 1f));
    }
}