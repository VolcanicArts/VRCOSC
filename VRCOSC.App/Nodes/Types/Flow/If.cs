// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

[Node("If", "Flow")]
public sealed class IfNode : Node
{
    public FlowInput FlowInput = new();
    public FlowOutput OnTrue = new();
    public FlowOutput OnFalse = new();

    public ValueInput<bool> Condition = new();

    protected override Task Process(IPulseContext c) => Condition.Read(c) ? OnTrue.Execute(c) : OnFalse.Execute(c);
}