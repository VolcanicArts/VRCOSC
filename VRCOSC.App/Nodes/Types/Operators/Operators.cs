// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Threading.Tasks;
using FontAwesome6;

namespace VRCOSC.App.Nodes.Types.Operators;

[Node("Equals", "Operators", EFontAwesomeIcon.Solid_Equals)]
public sealed class EqualsNode<T> : Node
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(EqualityComparer<T>.Default.Equals(A.Read(c), B.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Not Equals", "Operators", EFontAwesomeIcon.Solid_NotEqual)]
public sealed class NotEqualsNode<T> : Node
{
    public ValueInput<T> A = new();
    public ValueInput<T> B = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(!EqualityComparer<T>.Default.Equals(A.Read(c), B.Read(c)), c);
        return Task.CompletedTask;
    }
}