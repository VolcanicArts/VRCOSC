// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Strings;

[Node("Compare", "Strings")]
public sealed class StringCompareNode : Node
{
    public ValueInput<string> A = new();
    public ValueInput<string> B = new();
    public ValueInput<StringComparison> Comparison = new();
    public ValueOutput<int> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(string.Compare(A.Read(c), B.Read(c), Comparison.Read(c)), c);
    }
}

[Node("Join", "Strings")]
public class StringJoinNode : Node
{
    public ValueInput<string> Separator = new(defaultValue: string.Empty);
    public ValueInputList<string> Inputs = new();
    public ValueOutput<string> Output = new();

    protected override void Process(PulseContext c)
    {
        Output.Write(string.Join(Separator.Read(c), Inputs.Read(c)), c);
    }
}

[Node("Contains", "Strings")]
public class StringContainsNode : Node
{
    public ValueInput<string> Input = new(defaultValue: string.Empty);
    public ValueInput<string> Value = new(defaultValue: string.Empty);
    public ValueInput<StringComparison> Comparison = new();
    public ValueOutput<bool> Result = new();

    protected override void Process(PulseContext c)
    {
        var input = Input.Read(c);
        var value = Value.Read(c);

        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(value)) return;

        Result.Write(input.Contains(value, Comparison.Read(c)), c);
    }
}

[Node("To Upper", "Strings")]
[NodeCollapsed]
public class StringToUpperNode : Node
{
    public ValueInput<string> Input = new(defaultValue: string.Empty);
    public ValueOutput<string> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(Input.Read(c).ToUpper(), c);
    }
}

[Node("To Lower", "Strings")]
[NodeCollapsed]
public class StringToLowerNode : Node
{
    public ValueInput<string> Input = new(defaultValue: string.Empty);
    public ValueOutput<string> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(Input.Read(c).ToLower(), c);
    }
}

[Node("Is Null Or Empty", "Strings")]
[NodeCollapsed]
public sealed class StringIsNullOrEmptyNode : Node
{
    public ValueInput<string> Input = new(defaultValue: string.Empty);
    public ValueOutput<bool> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(string.IsNullOrEmpty(Input.Read(c)), c);
    }
}