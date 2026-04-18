// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Strings;

[Node("Compare", "Strings")]
public sealed class StringCompareNode : ValueComputeNode<int>
{
    public ValueInput<string?> A = new();
    public ValueInput<string?> B = new();
    public ValueInput<StringComparison> Comparison = new();

    protected override int ComputeValue(PulseContext c) => string.Compare(A.Read(c), B.Read(c), Comparison.Read(c));
}

[Node("Join", "Strings")]
public sealed class StringJoinNode : ValueComputeNode<string>
{
    public ValueInput<string?> Separator = new();
    public ValueInputList<string?> Inputs = new();

    protected override string ComputeValue(PulseContext c) => string.Join(Separator.Read(c), Inputs.Read(c));
}

[Node("Contains", "Strings")]
public sealed class StringContainsNode : ValueComputeNode<bool>
{
    public ValueInput<string?> Input = new();
    public ValueInput<string?> Value = new();
    public ValueInput<StringComparison> Comparison = new();

    protected override bool ComputeValue(PulseContext c)
    {
        var input = Input.Read(c);
        var value = Value.Read(c);

        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(value)) return false;

        return input.Contains(value, Comparison.Read(c));
    }
}

[Node("To Upper", "Strings")]
[NodeCollapsed]
public sealed class StringToUpperNode() : SimpleValueTransformNode<string?>(v => v?.ToUpper());

[Node("To Lower", "Strings")]
[NodeCollapsed]
public sealed class StringToLowerNode() : SimpleValueTransformNode<string?>(v => v?.ToLower());

[Node("Is Null Or Empty", "Strings")]
[NodeCollapsed]
public sealed class StringIsNullOrEmptyNode() : SimpleValueTransformNode<string?, bool>(string.IsNullOrEmpty);

[Node("Is Null Or WhiteSpace", "Strings")]
[NodeCollapsed]
public sealed class StringIsNullOrWhiteSpaceNode() : SimpleValueTransformNode<string?, bool>(string.IsNullOrWhiteSpace);

[Node("Length", "Strings")]
[NodeCollapsed]
public sealed class StringLengthNode() : SimpleValueTransformNode<string?, int>(v => v?.Length ?? 0);

[Node("Substring", "Strings")]
public sealed class StringSubstringNode : ValueComputeNode<string?>
{
    public ValueInput<string?> Input = new();
    public ValueInput<int> Index = new();
    public ValueInput<int> Length = new();

    protected override string? ComputeValue(PulseContext c)
    {
        var input = Input.Read(c);
        var index = Index.Read(c);
        var length = Length.Read(c);

        if (string.IsNullOrEmpty(input)) return input;

        try
        {
            return length == 0 ? input[index..] : input.Substring(index, length);
        }
        catch
        {
            return null;
        }
    }
}

[Node("Ends With", "Strings")]
public sealed class StringEndsWithNode : ValueComputeNode<bool>
{
    public ValueInput<string?> Input = new();
    public ValueInput<string?> Check = new();
    public ValueInput<StringComparison> Comparison = new();

    protected override bool ComputeValue(PulseContext c)
    {
        var input = Input.Read(c);
        var check = Check.Read(c);
        var comparison = Comparison.Read(c);

        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(check)) return false;

        return input.EndsWith(check, comparison);
    }
}

[Node("Starts With", "Strings")]
public sealed class StringStartsWithNode : ValueComputeNode<bool>
{
    public ValueInput<string?> Input = new();
    public ValueInput<string?> Check = new();
    public ValueInput<StringComparison> Comparison = new();

    protected override bool ComputeValue(PulseContext c)
    {
        var input = Input.Read(c);
        var check = Check.Read(c);
        var comparison = Comparison.Read(c);

        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(check)) return false;

        return input.StartsWith(check, comparison);
    }
}

[Node("Parse", "Strings")]
public sealed class StringParseNode<T> : Node, IFlowInput where T : IParsable<T>
{
    public FlowContinuation Success = new("On Success");
    public FlowContinuation Failed = new("On Failed");

    public ValueInput<string?> Input = new();
    public ValueInput<CultureInfo> Culture = new("Culture", CultureInfo.CurrentCulture);
    public ValueOutput<T> Output = new();

    protected override Task Process(PulseContext c)
    {
        if (T.TryParse(Input.Read(c), Culture.Read(c), out var parsedInput))
        {
            Output.Write(parsedInput, c);
            return Success.Execute(c);
        }

        Output.Write(default!, c);
        return Failed.Execute(c);
    }
}