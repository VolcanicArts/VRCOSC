// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Flow;

public abstract class FireIfBase(Func<bool, bool> checkCondition) : Node
{
    public FlowOutput Next = new();

    public ValueInput<bool> Condition = new();

    protected override Task Process(IPulseContext c) => Next.Execute(c);

    protected override bool ShouldProcess(IPulseContext c) => checkCondition(Condition.Read(c));
}

[Node("Fire If True", "Flow")]
public sealed class FireIfTrueNode() : FireIfBase(v => v);

[Node("Fire If False", "Flow")]
public sealed class FireIfFalseNode() : FireIfBase(v => !v);