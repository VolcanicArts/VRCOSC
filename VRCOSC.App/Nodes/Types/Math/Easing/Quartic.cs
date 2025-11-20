// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Quartic In", "Math/Easing")]
[NodeCollapsed]
public sealed class QuarticInNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Quartic.In(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Quartic Out", "Math/Easing")]
[NodeCollapsed]
public sealed class QuarticOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Quartic.Out(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Quartic InOut", "Math/Easing")]
[NodeCollapsed]
public sealed class QuarticInOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Quartic.InOut(In.Read(c)), c);
        return Task.CompletedTask;
    }
}