// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types;

[NodeCollapsed]
public abstract class ConstantNode<T> : Node
{
    private readonly T value;

    public ValueOutput<T> Output = new();

    protected ConstantNode(T value)
    {
        this.value = value;
    }

    protected override Task Process(PulseContext c)
    {
        Output.Write(value, c);
        return Task.CompletedTask;
    }
}