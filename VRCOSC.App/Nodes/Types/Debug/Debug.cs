// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

#if DEBUG
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Debug;

[Node("Log", "Debug")]
public sealed class LogNode<T>() : SimpleActionValueConsumeNode<T>(value => Logger.Log(value?.ToString() ?? "null", LoggingTarget.Information));

[Node("Everything", "Debug")]
public sealed class EverythingNode<T> : Node
{
    public FlowInput FlowInput1 = new("Flow 1");
    public FlowInputList FlowInputList = new("List");
    public FlowInput FlowInput2 = new("Flow 2");

    public FlowOutput FlowOutput1 = new("Flow 1");
    public FlowOutputList FlowOutputList = new("List");
    public FlowOutput FlowOutput2 = new("Flow 2");

    public ValueInput<T> ValueInput1 = new("Value 1");
    public ValueInputList<T> ValueInputList = new("List");
    public ValueInput<T> ValueInput2 = new("Value 2");

    public ValueOutput<T> ValueOutput1 = new("Value 1");
    public ValueOutputList<T> ValueOutputList = new("List");
    public ValueOutput<T> ValueOutput2 = new("Value 2");

    protected override Task Process(IPulseContext c) => Task.CompletedTask;
}
#endif