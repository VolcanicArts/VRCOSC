// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Strings;

[Node("Compare", "Strings")]
public sealed class StringCompareNode : Node
{
    public ValueInput<string> A = new();
    public ValueInput<string> B = new();
    public ValueInput<StringComparison> Comparison = new();
    public ValueOutput<int> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(string.Compare(A.Read(c), B.Read(c), Comparison.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Join", "Strings")]
public sealed class StringJoinNode : Node
{
    public ValueInput<string> Separator = new(defaultValue: string.Empty);
    public ValueInputList<string> Inputs = new();
    public ValueOutput<string> Output = new();

    protected override Task Process(PulseContext c)
    {
        Output.Write(string.Join(Separator.Read(c), Inputs.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Contains", "Strings")]
public sealed class StringContainsNode : Node
{
    public ValueInput<string> Input = new(defaultValue: string.Empty);
    public ValueInput<string> Value = new(defaultValue: string.Empty);
    public ValueInput<StringComparison> Comparison = new();
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        var value = Value.Read(c);

        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(value)) return Task.CompletedTask;

        Result.Write(input.Contains(value, Comparison.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("To Upper", "Strings")]
[NodeCollapsed]
public sealed class StringToUpperNode : Node
{
    public ValueInput<string> Input = new(defaultValue: string.Empty);
    public ValueOutput<string> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(Input.Read(c).ToUpper(), c);
        return Task.CompletedTask;
    }
}

[Node("To Lower", "Strings")]
[NodeCollapsed]
public sealed class StringToLowerNode : Node
{
    public ValueInput<string> Input = new(defaultValue: string.Empty);
    public ValueOutput<string> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(Input.Read(c).ToLower(), c);
        return Task.CompletedTask;
    }
}

[Node("Is Null Or Empty", "Strings")]
[NodeCollapsed]
public sealed class StringIsNullOrEmptyNode : Node
{
    public ValueInput<string> Input = new(defaultValue: string.Empty);
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(string.IsNullOrEmpty(Input.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Is Null Or WhiteSpace", "Strings")]
[NodeCollapsed]
public sealed class StringIsNullOrWhiteSpaceNode : Node
{
    public ValueInput<string> Input = new(defaultValue: string.Empty);
    public ValueOutput<bool> Result = new();

    protected override Task Process(PulseContext c)
    {
        Result.Write(string.IsNullOrWhiteSpace(Input.Read(c)), c);
        return Task.CompletedTask;
    }
}

[Node("Length", "Strings")]
[NodeCollapsed]
public sealed class StringLengthNode : Node
{
    public ValueInput<string> Input = new(defaultValue: string.Empty);
    public ValueOutput<int> Length = new();

    protected override Task Process(PulseContext c)
    {
        Length.Write(Input.Read(c).Length, c);
        return Task.CompletedTask;
    }
}

[Node("Substring", "Strings")]
public sealed class StringSubstringNode : Node
{
    public ValueInput<string> Input = new(defaultValue: string.Empty);
    public ValueInput<int> Index = new();
    public ValueInput<int> Length = new();

    public ValueOutput<string> Output = new();

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        var index = Index.Read(c);
        var length = Length.Read(c);

        try
        {
            Output.Write(length == 0 ? input.Substring(index) : input.Substring(index, length), c);
        }
        catch
        {
            Output.Write(string.Empty, c);
        }

        return Task.CompletedTask;
    }
}

[Node("Ends With", "Strings")]
public sealed class StringEndsWithNode : Node
{
    public ValueInput<string> Input = new(defaultValue: string.Empty);
    public ValueInput<string> Check = new(defaultValue: string.Empty);
    public ValueInput<StringComparison> Comparison = new();

    public ValueOutput<bool> Output = new();

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        var check = Check.Read(c);
        var comparison = Comparison.Read(c);

        Output.Write(input.EndsWith(check, comparison), c);

        return Task.CompletedTask;
    }
}

[Node("Starts With", "Strings")]
public sealed class StringStartsWithNode : Node
{
    public ValueInput<string> Input = new(defaultValue: string.Empty);
    public ValueInput<string> Check = new(defaultValue: string.Empty);
    public ValueInput<StringComparison> Comparison = new();

    public ValueOutput<bool> Output = new();

    protected override Task Process(PulseContext c)
    {
        var input = Input.Read(c);
        var check = Check.Read(c);
        var comparison = Comparison.Read(c);

        Output.Write(input.StartsWith(check, comparison), c);

        return Task.CompletedTask;
    }
}