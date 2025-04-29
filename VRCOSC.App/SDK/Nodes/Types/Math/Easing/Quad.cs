// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Math.Easing;

using VRCOSC.App.Utils;

[Node("Quad In", "Math/Easing")]
public sealed class QuadInNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Easing.Quadratic.In(float.Clamp(x, 0f, 1f));
    }
}

[Node("Quad Out", "Math/Easing")]
public sealed class QuadOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Easing.Quadratic.Out(float.Clamp(x, 0f, 1f));
    }
}

[Node("Quad InOut", "Math/Easing")]
public sealed class QuadInOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Easing.Quadratic.InOut(float.Clamp(x, 0f, 1f));
    }
}