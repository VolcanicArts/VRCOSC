// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.SDK.Nodes.Types.Strings;

[Node("Join", "Strings")]
public class StringJoinNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Separator")] string? separator,
        [NodeValue("Inputs")] [NodeVariableSize] string[] inputs,
        [NodeValue("String")] ref string outString
    )
    {
        outString = string.Join(separator, inputs);
    }
}

[Node("Contains", "Strings")]
public class StringContainsNode : Node
{
    [NodeProcess]
    private void process
    (
        [NodeValue("Input")] string? input,
        [NodeValue("Value")] string? value,
        [NodeValue("Comparison")] StringComparison comparison,
        [NodeValue("Result")] ref bool result
    )
    {
        if (input is null || value is null) return;

        result = input.Contains(value, comparison);
    }
}

[Node("To Upper", "Strings")]
public class StringToUpperNode : Node
{
    [NodeProcess]
    private void process(string? input, ref string? result)
    {
        result = input?.ToUpper();
    }
}

[Node("To Lower", "Strings")]
public class StringToLowerNode : Node
{
    [NodeProcess]
    private void process(string? input, ref string? result)
    {
        result = input?.ToLower();
    }
}