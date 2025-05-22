// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Strings;

[Node("Join", "Strings")]
public class StringJoinNode : Node
{
    public ValueInput<string> Separator = new(string.Empty);
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
    public ValueInput<string> Input = new(string.Empty);
    public ValueInput<string> Value = new(string.Empty);
    public ValueInput<StringComparison> Comparison = new(StringComparison.InvariantCulture);
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
public class StringToUpperNode : Node
{
    public ValueInput<string> Input = new(string.Empty);
    public ValueOutput<string> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(Input.Read(c).ToUpper(), c);
    }
}

[Node("To Lower", "Strings")]
public class StringToLowerNode : Node
{
    public ValueInput<string> Input = new(string.Empty);
    public ValueOutput<string> Result = new();

    protected override void Process(PulseContext c)
    {
        Result.Write(Input.Read(c).ToLower(), c);
    }
}