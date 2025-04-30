// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Circular In", "Math/Easing")]
public sealed class CircularInNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Utils.Easing.Circular.In(float.Clamp(x, 0f, 1f));
    }
}

[Node("Circular Out", "Math/Easing")]
public sealed class CircularOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Utils.Easing.Circular.Out(float.Clamp(x, 0f, 1f));
    }
}

[Node("Circular InOut", "Math/Easing")]
public sealed class CircularInOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Utils.Easing.Circular.InOut(float.Clamp(x, 0f, 1f));
    }
}