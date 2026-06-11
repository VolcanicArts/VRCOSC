// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.Nodes.Types.Inputs;

[Node("Value")]
public class ValueNode<T> : ValueComputeNode<T>
{
    [InputMode(InputModes.Inline)]
    public ValueInput<T> Value = new();

    protected override T ComputeValue(IPulseContext c) => Value.Read(c);
}