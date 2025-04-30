// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Exponential In", "Math/Easing")]
public sealed class ExponentialInNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Utils.Easing.Exponential.In(float.Clamp(x, 0f, 1f));
    }
}

[Node("Exponential Out", "Math/Easing")]
public sealed class ExponentialOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Utils.Easing.Exponential.Out(float.Clamp(x, 0f, 1f));
    }
}

[Node("Exponential InOut", "Math/Easing")]
public sealed class ExponentialInOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Utils.Easing.Exponential.InOut(float.Clamp(x, 0f, 1f));
    }
}