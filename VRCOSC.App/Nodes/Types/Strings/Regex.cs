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

    public FlowContinuation OnMatchHit = new("On Match Hit");
    public FlowContinuation OnMatchMiss = new("On Match Miss");

    public ValueInput<string> StrInput = new("String");
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
public sealed class RegexMatchGroupsNode : Node
{
    public ValueInput<Match> Match = new();
    public ValueOutput<IReadOnlyList<Group>> Groups = new();

    protected override Task Process(PulseContext c)
    {
        var match = Match.Read(c);
        if (match is null) return Task.CompletedTask;

        Groups.Write(match.Groups, c);
        return Task.CompletedTask;
    }
}

[Node("Regex Group Captures", "Strings/Regex")]
public sealed class RegexGroupCapturesNode : Node
{
    public ValueInput<Group> Group = new();
    public ValueOutput<IReadOnlyList<Capture>> Captures = new();

    protected override Task Process(PulseContext c)
    {
        var group = Group.Read(c);
        if (group is null) return Task.CompletedTask;

        Captures.Write(group.Captures, c);
        return Task.CompletedTask;
    }
}

[Node("Regex Capture Value", "Strings/Regex")]
public sealed class RegexCaptureValueNode<T> : Node, IFlowInput
{
    public FlowContinuation OnSuccess = new("On Success");
    public FlowContinuation OnFail = new("On Fail");

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