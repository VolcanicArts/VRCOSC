// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Back In", "Math/Easing")]
[NodeCollapsed]
public sealed class BackInNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Back.In(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Back Out", "Math/Easing")]
[NodeCollapsed]
public sealed class BackOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Back.Out(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Back InOut", "Math/Easing")]
[NodeCollapsed]
public sealed class BackInOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Back.InOut(In.Read(c)), c);
        return Task.CompletedTask;
    }
}