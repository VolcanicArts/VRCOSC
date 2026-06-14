// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("Multiplex", "Flow")]
public sealed class FlowMultiplexNode : Node
{
    public FlowInput FlowInput = new();
    public FlowOutput Default = new("Default");
    public FlowOutputList FlowOutputs = new("Flows");

    public ValueInput<int> Index = new();

    protected override Task Process(IPulseContext c)
    {
        var index = Index.Read(c);
        return index >= 0 && index < FlowOutputs.Count ? FlowOutputs.Execute(index, c) : Default.Execute(c);
    }
}

[Node("Demultiplex", "Flow")]
public sealed class FlowDemultiplexNode : Node
{
    public FlowInputList FlowInputs = new("Flows");
    public FlowOutput Next = new();

    public ValueInput<int> Index = new();

    protected override Task Process(IPulseContext c)
    {
        var index = Index.Read(c);
        return FlowInputs.IsSource(index, c) ? Next.Execute(c) : Task.CompletedTask;
    }
}