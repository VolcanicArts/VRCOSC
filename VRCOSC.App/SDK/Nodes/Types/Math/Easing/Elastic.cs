// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.Nodes.Types.Math.Easing;

using VRCOSC.App.Utils;

[Node("Elastic In", "Math/Easing")]
public sealed class ElasticInNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Easing.Elastic.In(float.Clamp(x, 0f, 1f));
    }
}

[Node("Elastic Out", "Math/Easing")]
public sealed class ElasticOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Easing.Elastic.Out(float.Clamp(x, 0f, 1f));
    }
}

[Node("Elastic InOut", "Math/Easing")]
public sealed class ElasticInOutNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue] float x,
        [NodeValue] ref float outY
    )
    {
        outY = Easing.Elastic.InOut(float.Clamp(x, 0f, 1f));
    }
}