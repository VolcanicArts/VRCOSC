// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types;

[NodeCollapsed]
public abstract class ValueOutputNode<T> : Node
{
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        Output.Write(GetValue(), c);
        return Task.CompletedTask;
    }

    protected abstract T GetValue();
}