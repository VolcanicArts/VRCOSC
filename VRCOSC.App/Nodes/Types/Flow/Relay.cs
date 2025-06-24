// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Relay", "Utility")]
public sealed class RelayNode<T> : Node
{
    public ValueInput<T> Input = new();
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        Output.Write(Input.Read(c), c);
        return Task.CompletedTask;
    }
}