// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.SDK.Nodes.Types;

public abstract class Node
{
    public NodeField NodeField { get; }
    public Guid Id { get; } = Guid.NewGuid();

    protected Node(NodeField nodeField)
    {
        NodeField = nodeField;
    }

    protected void SetOutputValue(int slot, object value) => NodeField.SetOutputValue(this, slot, value);
}

[NodeValue]
[NodeFlow(false, 0)]
public class PrintNode : Node
{
    public PrintNode(NodeField nodeField)
        : base(nodeField)
    {
    }

    [NodeProcess]
    private void execute(string? inputA)
    {
        Console.WriteLine(inputA);
    }
}