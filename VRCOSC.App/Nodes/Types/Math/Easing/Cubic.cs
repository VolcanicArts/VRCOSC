// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Cubic In", "Math/Easing")]
public sealed class CubicInNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] Ref<float> outY
    )
    {
        outY.Value = Utils.Easing.Cubic.In(float.Clamp(x, 0f, 1f));
    }
}

[Node("Cubic Out", "Math/Easing")]
public sealed class CubicOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] Ref<float> outY
    )
    {
        outY.Value = Utils.Easing.Cubic.Out(float.Clamp(x, 0f, 1f));
    }
}

[Node("Cubic InOut", "Math/Easing")]
public sealed class CubicInOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] Ref<float> outY
    )
    {
        outY.Value = Utils.Easing.Cubic.InOut(float.Clamp(x, 0f, 1f));
    }
}