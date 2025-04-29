// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Math.Easing;

using VRCOSC.App.Utils;

[Node("Back In", "Math/Easing")]
public sealed class BackInNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Easing.Back.In(float.Clamp(x, 0f, 1f));
    }
}

[Node("Back Out", "Math/Easing")]
public sealed class BackOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Easing.Back.Out(float.Clamp(x, 0f, 1f));
    }
}

[Node("Back InOut", "Math/Easing")]
public sealed class BackInOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Easing.Back.InOut(float.Clamp(x, 0f, 1f));
    }
}