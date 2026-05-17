// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text.RegularExpressions;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Strings;

[Node("Regex Match", "Strings/Regex")]
public sealed class RegexMatchNode : TryValueComputeNode<Match>
{
    public ValueInput<string?> Regex = new();
    public ValueInput<string?> Input = new("String");
    public ValueInput<RegexOptions> Options = new(modes: ValueInputMode.Connection);

    protected override Result<Match> TryComputeValue(IPulseContext c)
    {
        var input = Input.Read(c);
        if (input is null) return Result<Match>.Fail();

        var regex = Regex.Read(c);
        if (string.IsNullOrEmpty(regex)) return Result<Match>.Fail();

        try
        {
            var options = Options.Read(c);
            return System.Text.RegularExpressions.Regex.Match(input, regex, options);
        }
        catch
        {
            return Result<Match>.Fail();
        }
    }
}

[Node("Regex Matches", "Strings/Regex")]
public sealed class RegexMatchesNode : TryValueComputeNode<MatchCollection>
{
    public ValueInput<string> RegexStr = new("Regex");
    public ValueInput<string?> Input = new("String");
    public ValueInput<RegexOptions> Options = new(modes: ValueInputMode.Connection);

    protected override Result<MatchCollection> TryComputeValue(IPulseContext c)
    {
        var input = Input.Read(c);
        if (input is null) return Result<MatchCollection>.Fail();

        try
        {
            var options = Options.Read(c);
            return Regex.Matches(input, RegexStr.Read(c), options);
        }
        catch
        {
            return Result<MatchCollection>.Fail();
        }
    }
}

[Node("Regex Is Match", "Strings/Regex")]
public sealed class RegexIsMatchNode : ValueSourceNode<bool>
{
    public ValueInput<string> RegexStr = new("Regex");
    public ValueInput<string?> Input = new("String");
    public ValueInput<RegexOptions> Options = new(modes: ValueInputMode.Connection);

    protected override bool ComputeValue(IPulseContext c)
    {
        var input = Input.Read(c);
        if (input is null) return false;

        try
        {
            var options = Options.Read(c);
            return Regex.IsMatch(input, RegexStr.Read(c), options);
        }
        catch
        {
            return false;
        }
    }
}

[Node("Regex Replace", "Strings/Regex")]
public sealed class RegexReplaceNode : TryValueComputeNode<string>
{
    public ValueInput<string> RegexStr = new("Regex");
    public ValueInput<string?> Input = new("String");
    public ValueInput<string> Replacement = new();
    public ValueInput<RegexOptions> Options = new(modes: ValueInputMode.Connection);

    protected override Result<string> TryComputeValue(IPulseContext c)
    {
        var input = Input.Read(c);
        if (input is null) return Result<string>.Fail();

        try
        {
            var replacement = Replacement.Read(c);
            var options = Options.Read(c);
            return Regex.Replace(input, RegexStr.Read(c), replacement, options);
        }
        catch
        {
            return Result<string>.Fail();
        }
    }
}

[Node("Regex Split", "Strings/Regex")]
public sealed class RegexSplitNode : TryValueComputeNode<string[]>
{
    public ValueInput<string> RegexStr = new("Regex");
    public ValueInput<string?> Input = new("String");
    public ValueInput<RegexOptions> Options = new(modes: ValueInputMode.Connection);

    protected override Result<string[]> TryComputeValue(IPulseContext c)
    {
        var input = Input.Read(c);
        if (input is null) return Result<string[]>.Fail();

        try
        {
            var options = Options.Read(c);
            return Regex.Split(input, RegexStr.Read(c), options);
        }
        catch
        {
            return Result<string[]>.Fail();
        }
    }
}

[Node("Match Success", "Strings/Regex")]
[NodeCollapsed]
public sealed class MatchSuccessNode() : SimpleValueTransformNode<Match?, bool>(m => m?.Success ?? false);

[Node("Match Value", "Strings/Regex")]
[NodeCollapsed]
public sealed class MatchValueNode() : SimpleValueTransformNode<Match?, string>(m => m?.Value ?? string.Empty);

[Node("Match Index", "Strings/Regex")]
[NodeCollapsed]
public sealed class MatchIndexNode() : SimpleValueTransformNode<Match?, int>(m => m?.Index ?? -1);

[Node("Match Length", "Strings/Regex")]
[NodeCollapsed]
public sealed class MatchLengthNode() : SimpleValueTransformNode<Match?, int>(m => m?.Length ?? 0);

[Node("Match Groups", "Strings/Regex")]
[NodeCollapsed]
public sealed class MatchGroupsNode() : SimpleValueTransformNode<Match?, GroupCollection?>(m => m?.Groups);

[Node("Get Group By Index", "Strings/Regex")]
public sealed class GetGroupByIndexNode : TryValueComputeNode<Group>
{
    public ValueInput<GroupCollection?> Groups = new();
    public ValueInput<int> Index = new();

    protected override Result<Group> TryComputeValue(IPulseContext c)
    {
        var groups = Groups.Read(c);
        var index = Index.Read(c);

        if (groups is null || index < 0 || index >= groups.Count)
            return Result<Group>.Fail();

        return groups[index];
    }
}

[Node("Get Group By Name", "Strings/Regex")]
public sealed class GetGroupByNameNode : TryValueComputeNode<Group>
{
    public ValueInput<GroupCollection?> Groups = new();
    public ValueInput<string> Name = new();

    protected override Result<Group> TryComputeValue(IPulseContext c)
    {
        var groups = Groups.Read(c);
        var name = Name.Read(c);

        if (groups is null || string.IsNullOrEmpty(name))
            return Result<Group>.Fail();

        try
        {
            var group = groups[name];
            return group.Success ? group : Result<Group>.Fail();
        }
        catch
        {
            return Result<Group>.Fail();
        }
    }
}

[Node("Group Collection Count", "Strings/Regex")]
[NodeCollapsed]
public sealed class GroupCollectionCountNode() : SimpleValueTransformNode<GroupCollection?, int>(g => g?.Count ?? 0);

[Node("Group Success", "Strings/Regex")]
[NodeCollapsed]
public sealed class GroupSuccessNode() : SimpleValueTransformNode<Group?, bool>(g => g?.Success ?? false);

[Node("Group Value", "Strings/Regex")]
[NodeCollapsed]
public sealed class GroupValueNode() : SimpleValueTransformNode<Group?, string>(g => g?.Value ?? string.Empty);

[Node("Group Index", "Strings/Regex")]
[NodeCollapsed]
public sealed class GroupIndexNode() : SimpleValueTransformNode<Group?, int>(g => g?.Index ?? -1);

[Node("Group Length", "Strings/Regex")]
[NodeCollapsed]
public sealed class GroupLengthNode() : SimpleValueTransformNode<Group?, int>(g => g?.Length ?? 0);

[Node("Group Name", "Strings/Regex")]
[NodeCollapsed]
public sealed class GroupNameNode() : SimpleValueTransformNode<Group?, string>(g => g?.Name ?? string.Empty);

[Node("Group Captures", "Strings/Regex")]
[NodeCollapsed]
public sealed class GroupCapturesNode() : SimpleValueTransformNode<Group?, CaptureCollection?>(g => g?.Captures);

[Node("Get Capture By Index", "Strings/Regex")]
public sealed class GetCaptureByIndexNode : TryValueComputeNode<Capture>
{
    public ValueInput<CaptureCollection?> Captures = new();
    public ValueInput<int> Index = new();

    protected override Result<Capture> TryComputeValue(IPulseContext c)
    {
        var captures = Captures.Read(c);
        var index = Index.Read(c);

        if (captures is null || index < 0 || index >= captures.Count)
            return Result<Capture>.Fail();

        return captures[index];
    }
}

[Node("Capture Collection Count", "Strings/Regex")]
[NodeCollapsed]
public sealed class CaptureCollectionCountNode() : SimpleValueTransformNode<CaptureCollection?, int>(c => c?.Count ?? 0);

[Node("Capture Value", "Strings/Regex")]
[NodeCollapsed]
public sealed class CaptureValueNode() : SimpleValueTransformNode<Capture?, string>(c => c?.Value ?? string.Empty);

[Node("Capture Index", "Strings/Regex")]
[NodeCollapsed]
public sealed class CaptureIndexNode() : SimpleValueTransformNode<Capture?, int>(c => c?.Index ?? -1);

[Node("Capture Length", "Strings/Regex")]
[NodeCollapsed]
public sealed class CaptureLengthNode() : SimpleValueTransformNode<Capture?, int>(c => c?.Length ?? 0);

[Node("Get Match By Index", "Strings/Regex")]
public sealed class GetMatchByIndexNode : TryValueComputeNode<Match>
{
    public ValueInput<MatchCollection?> Matches = new();
    public ValueInput<int> Index = new();

    protected override Result<Match> TryComputeValue(IPulseContext c)
    {
        var matches = Matches.Read(c);
        var index = Index.Read(c);

        if (matches is null || index < 0 || index >= matches.Count)
            return Result<Match>.Fail();

        return matches[index];
    }
}

[Node("Match Collection Count", "Strings/Regex")]
[NodeCollapsed]
public sealed class MatchCollectionCountNode() : SimpleValueTransformNode<MatchCollection?, int>(m => m?.Count ?? 0);