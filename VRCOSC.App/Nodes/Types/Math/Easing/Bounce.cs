// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Bounce In", "Math/Easing")]
[NodeCollapsed]
public sealed class BounceInNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Bounce.In(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Bounce Out", "Math/Easing")]
[NodeCollapsed]
public sealed class BounceOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Bounce.Out(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Bounce InOut", "Math/Easing")]
[NodeCollapsed]
public sealed class BounceInOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Bounce.InOut(In.Read(c)), c);
        return Task.CompletedTask;
    }
}