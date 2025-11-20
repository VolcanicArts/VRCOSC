// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Sine In", "Math/Easing")]
[NodeCollapsed]
public sealed class SineInNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Sinusoidal.In(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Sine Out", "Math/Easing")]
[NodeCollapsed]
public sealed class SineOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Sinusoidal.Out(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Sine InOut", "Math/Easing")]
[NodeCollapsed]
public sealed class SineInOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Sinusoidal.InOut(In.Read(c)), c);
        return Task.CompletedTask;
    }
}