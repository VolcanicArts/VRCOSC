// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Sine In", "Math/Easing")]
[NodeCollapsed]
public sealed class SineInNode : Node
{
    public ValueInput<float> X = new();
    public ValueOutput<float> Y = new();

    protected override Task Process(PulseContext c)
    {
        Y.Write(Utils.Easing.Sinusoidal.In(float.Clamp(X.Read(c), 0f, 1f)), c);
        return Task.CompletedTask;
    }
}

[Node("Sine Out", "Math/Easing")]
[NodeCollapsed]
public sealed class SineOutNode : Node
{
    public ValueInput<float> X = new();
    public ValueOutput<float> Y = new();

    protected override Task Process(PulseContext c)
    {
        Y.Write(Utils.Easing.Sinusoidal.Out(float.Clamp(X.Read(c), 0f, 1f)), c);
        return Task.CompletedTask;
    }
}

[Node("Sine InOut", "Math/Easing")]
[NodeCollapsed]
public sealed class SineInOutNode : Node
{
    public ValueInput<float> X = new();
    public ValueOutput<float> Y = new();

    protected override Task Process(PulseContext c)
    {
        Y.Write(Utils.Easing.Sinusoidal.InOut(float.Clamp(X.Read(c), 0f, 1f)), c);
        return Task.CompletedTask;
    }
}