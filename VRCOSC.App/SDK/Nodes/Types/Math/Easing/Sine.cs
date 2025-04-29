// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Math.Easing;

using VRCOSC.App.Utils;

[Node("Sine In", "Math/Easing")]
public sealed class SineInNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] Ref<float> outY
    )
    {
        outY.Value = Easing.Sinusoidal.In(float.Clamp(x, 0f, 1f));
    }
}

[Node("Sine Out", "Math/Easing")]
public sealed class SineOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] Ref<float> outY
    )
    {
        outY.Value = Easing.Sinusoidal.Out(float.Clamp(x, 0f, 1f));
    }
}

[Node("Sine InOut", "Math/Easing")]
public sealed class SineInOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] Ref<float> outY
    )
    {
        outY.Value = Easing.Sinusoidal.InOut(float.Clamp(x, 0f, 1f));
    }
}