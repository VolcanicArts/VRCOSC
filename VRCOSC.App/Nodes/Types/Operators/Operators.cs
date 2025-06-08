// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using FontAwesome6;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Operators;

[Node("Equals", "Operators", EFontAwesomeIcon.Solid_Equals)]
public sealed class EqualsNode<T> : Node
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<bool> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(EqualityComparer<T>.Default.Equals(A.Read(c), B.Read(c)), c);
    }
}

[Node("Not Equals", "Operators", EFontAwesomeIcon.Solid_NotEqual)]
public sealed class NotEqualsNode<T> : Node
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<bool> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(!EqualityComparer<T>.Default.Equals(A.Read(c), B.Read(c)), c);
    }
}