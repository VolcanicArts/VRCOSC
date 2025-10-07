// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Quintic In", "Math/Easing")]
[NodeCollapsed]
public sealed class QuinticInNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Quintic.In(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Quintic Out", "Math/Easing")]
[NodeCollapsed]
public sealed class QuinticOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Quintic.Out(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Quintic InOut", "Math/Easing")]
[NodeCollapsed]
public sealed class QuinticInOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Quintic.InOut(In.Read(c)), c);
        return Task.CompletedTask;
    }
}