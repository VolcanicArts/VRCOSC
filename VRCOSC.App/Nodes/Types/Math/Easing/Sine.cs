// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Sine In", "Math/Easing")]
[NodeCollapsed]
public sealed class SineInNode : Node
{
    public ValueInput<float> X = new();
    public ValueOutput<float> Y = new();

    protected override void Process(PulseContext c)
    {
        Y.Write(Utils.Easing.Sinusoidal.In(float.Clamp(X.Read(c), 0f, 1f)), c);
    }
}

[Node("Sine Out", "Math/Easing")]
[NodeCollapsed]
public sealed class SineOutNode : Node
{
    public ValueInput<float> X = new();
    public ValueOutput<float> Y = new();

    protected override void Process(PulseContext c)
    {
        Y.Write(Utils.Easing.Sinusoidal.Out(float.Clamp(X.Read(c), 0f, 1f)), c);
    }
}

[Node("Sine InOut", "Math/Easing")]
[NodeCollapsed]
public sealed class SineInOutNode : Node
{
    public ValueInput<float> X = new();
    public ValueOutput<float> Y = new();

    protected override void Process(PulseContext c)
    {
        Y.Write(Utils.Easing.Sinusoidal.InOut(float.Clamp(X.Read(c), 0f, 1f)), c);
    }
}