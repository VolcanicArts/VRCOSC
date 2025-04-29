// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Math.Easing;

using VRCOSC.App.Utils;

[Node("Bounce In", "Math/Easing")]
public sealed class BounceInNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Easing.Bounce.In(float.Clamp(x, 0f, 1f));
    }
}

[Node("Bounce Out", "Math/Easing")]
public sealed class BounceOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Easing.Bounce.Out(float.Clamp(x, 0f, 1f));
    }
}

[Node("Bounce InOut", "Math/Easing")]
public sealed class BounceInOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Easing.Bounce.InOut(float.Clamp(x, 0f, 1f));
    }
}