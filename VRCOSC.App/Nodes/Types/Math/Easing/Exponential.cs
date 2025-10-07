// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Exponential In", "Math/Easing")]
[NodeCollapsed]
public sealed class ExponentialInNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Exponential.In(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Exponential Out", "Math/Easing")]
[NodeCollapsed]
public sealed class ExponentialOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Exponential.Out(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Exponential InOut", "Math/Easing")]
[NodeCollapsed]
public sealed class ExponentialInOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Exponential.InOut(In.Read(c)), c);
        return Task.CompletedTask;
    }
}