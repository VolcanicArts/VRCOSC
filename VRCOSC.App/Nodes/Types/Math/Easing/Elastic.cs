// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Math.Easing;

[Node("Elastic In", "Math/Easing")]
[NodeCollapsed]
public sealed class ElasticInNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Elastic.In(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Elastic Out", "Math/Easing")]
[NodeCollapsed]
public sealed class ElasticOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Elastic.Out(In.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Elastic InOut", "Math/Easing")]
[NodeCollapsed]
public sealed class ElasticInOutNode<T> : Node where T : IFloatingPointIeee754<T>
{
    public ValueInput<T> In = new();
    public ValueOutput<T> Out = new();

    protected override Task Process(PulseContext c)
    {
        Out.Write(Utils.Easing.Elastic.InOut(In.Read(c)), c);
        return Task.CompletedTask;
    }
}