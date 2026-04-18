// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Strings;

[Node("Regex Match", "Strings/Regex")]
public sealed class RegexMatchNode : Node, IFlowInput, IHasTextProperty
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public FlowContinuation OnMatchHit = new();
    public FlowContinuation OnMatchMiss = new();

    public ValueInput<string?> StrInput = new("String");
    public ValueOutput<Match> Result = new();

    protected override Task Process(PulseContext c)
    {
        var strInput = StrInput.Read(c);
        if (strInput is null) return OnMatchMiss.Execute(c);

        try
        {
            var result = Regex.Match(strInput, Text);
            Result.Write(result, c);
            return OnMatchHit.Execute(c);
        }
        catch
        {
            return OnMatchMiss.Execute(c);
        }
    }
}

[Node("Regex Match Groups", "Strings/Regex")]
public sealed class RegexMatchGroupsNode() : ValueTransformNode<Match?, IReadOnlyList<Group>>("Match", "Groups")
{
    protected override IReadOnlyList<Group> TransformValue(Match? value) => value is not null ? value.Groups : new List<Group>();
}

[Node("Regex Group Captures", "Strings/Regex")]
public sealed class RegexGroupCapturesNode() : ValueTransformNode<Group?, IReadOnlyList<Capture>>("Group", "Captures")
{
    protected override IReadOnlyList<Capture> TransformValue(Group? value) => value is not null ? value.Captures : new List<Capture>();
}

[Node("Regex Capture Value", "Strings/Regex")]
public sealed class RegexCaptureValueNode<T> : Node, IFlowInput
{
    public FlowContinuation OnSuccess = new();
    public FlowContinuation OnFail = new();

    public ValueInput<Capture> Capture = new();
    public ValueOutput<T> Value = new();

    protected override Task Process(PulseContext c)
    {
        var capture = Capture.Read(c);
        if (capture is null) return OnFail.Execute(c);

        var value = capture.Value;

        try
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (!converter.CanConvertFrom(typeof(string))) return OnFail.Execute(c);

            Value.Write((T)converter.ConvertFrom(value)!, c);
        }
        catch
        {
            return OnFail.Execute(c);
        }

        return OnSuccess.Execute(c);
    }
}