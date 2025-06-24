// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Is Null", "Utility")]
[NodeCollapsed]
public sealed class IsNullNode<T> : Node
{
    public ValueInput<T> Input = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(Input.Read(c) is null, c);
        return Task.CompletedTask;
    }
}

[Node("Is Not Null", "Utility")]
[NodeCollapsed]
public sealed class IsNotNullNode<T> : Node
{
    public ValueInput<T> Input = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(Input.Read(c) is not null, c);
        return Task.CompletedTask;
    }
}