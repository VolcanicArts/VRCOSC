// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using FontAwesome6;

namespace VRCOSC.App.Nodes.Types.Operators;

[Node("Equals", "Operators")]
[NodeCollapsed(EFontAwesomeIcon.Solid_Equals)]
public sealed class EqualsNode<T>() : SimpleResultComputeNode<T, bool>(EqualityComparer<T>.Default.Equals);

[Node("Not Equals", "Operators")]
[NodeCollapsed(EFontAwesomeIcon.Solid_NotEqual)]
public sealed class NotEqualsNode<T>() : SimpleResultComputeNode<T, bool>((a, b) => !EqualityComparer<T>.Default.Equals(a, b));

[Node("Equals Any", "Operators")]
public sealed class EqualsAnyNode<T> : ValueComputeNode<bool>
{
    [InputMode(InputModes.Connection)]
    public ValueInput<T> Value = new();

    public ValueInputList<T> Values = new();

    protected override bool ComputeValue(IPulseContext c)
    {
        var value = Value.Read(c);
        return Values.Read(c).Any(v => EqualityComparer<T>.Default.Equals(v, value));
    }
}