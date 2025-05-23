// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Is Null", "Utility")]
[NodeCollapsed]
public sealed class IsNullNode<T> : Node
{
    public ValueInput<T> Input = new();
    public ValueOutput<bool> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(Input.Read(c) is null, c);
    }
}

[Node("Is Not Null", "Utility")]
[NodeCollapsed]
public sealed class IsNotNullNode<T> : Node
{
    public ValueInput<T> Input = new();
    public ValueOutput<bool> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(Input.Read(c) is not null, c);
    }
}