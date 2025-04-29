// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Math.Easing;

using VRCOSC.App.Utils;

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
        outY = Easing.Exponential.In(float.Clamp(x, 0f, 1f));
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
        outY = Easing.Exponential.Out(float.Clamp(x, 0f, 1f));
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
        outY = Easing.Exponential.InOut(float.Clamp(x, 0f, 1f));
    }
}