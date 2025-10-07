// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Cubic In", "Math/Easing")]
[NodeCollapsed]
public sealed class CubicInNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Cubic.In(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Cubic Out", "Math/Easing")]
[NodeCollapsed]
public sealed class CubicOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Cubic.Out(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Cubic InOut", "Math/Easing")]
[NodeCollapsed]
public sealed class CubicInOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Cubic.InOut(In.Read(c)), c);
        return Task.CompletedTask;
    }
}