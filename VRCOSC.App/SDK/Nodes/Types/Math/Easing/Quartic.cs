// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Math.Easing;

using VRCOSC.App.Utils;

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
        outY = Easing.Quartic.In(float.Clamp(x, 0f, 1f));
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
        outY = Easing.Quartic.Out(float.Clamp(x, 0f, 1f));
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
        outY = Easing.Quartic.InOut(float.Clamp(x, 0f, 1f));
    }
}