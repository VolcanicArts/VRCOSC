// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Circular In", "Math/Easing")]
[NodeCollapsed]
public sealed class CircularInNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Circular.In(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Circular Out", "Math/Easing")]
[NodeCollapsed]
public sealed class CircularOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Circular.Out(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Circular InOut", "Math/Easing")]
[NodeCollapsed]
public sealed class CircularInOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Circular.InOut(In.Read(c)), c);
        return Task.CompletedTask;
    }
}