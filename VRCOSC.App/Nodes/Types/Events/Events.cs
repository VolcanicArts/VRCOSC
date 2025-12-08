// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.SDK.VRChat;

namespace VRCOSC.App.Nodes.Types.Events;

[Node("On Start", "Events")]
public sealed class OnStartNode : Node, INodeEventHandler
{
    public FlowCall OnStart = new("On Start");

    protected override async Task Process(PulseContext c)
    {
        await OnStart.Execute(c);
    }

    public Task<bool> HandleNodeStart(PulseContext c) => Task.FromResult(true);
}

[Node("On Stop", "Events")]
public sealed class OnStopNode : Node, INodeEventHandler
{
    public FlowCall OnStop = new("On Stop");

    protected override async Task Process(PulseContext c)
    {
        await OnStop.Execute(c);
    }

    public Task<bool> HandleNodeStop(PulseContext c) => Task.FromResult(true);
}

[Node("On Instance Joined", "Events")]
public sealed class OnInstanceJoinedNode : Node, INodeEventHandler
{
    public FlowCall OnInstanceJoined = new();

    public ValueOutput<string> WorldId = new("World Id");

    protected override async Task Process(PulseContext c)
    {
        await OnInstanceJoined.Execute(c);
    }

    public Task<bool> HandleOnInstanceJoined(PulseContext c, VRChatClientEventInstanceJoined eventArgs)
    {
        WorldId.Write(eventArgs.WorldId, c);
        return Task.FromResult(true);
    }
}

[Node("On Instance Left", "Events")]
public sealed class OnInstanceLeftNode : Node, INodeEventHandler
{
    public FlowCall OnInstanceLeft = new();

    protected override async Task Process(PulseContext c)
    {
        await OnInstanceLeft.Execute(c);
    }

    public Task<bool> HandleOnInstanceLeft(PulseContext c, VRChatClientEventInstanceLeft eventArgs) => Task.FromResult(true);
}

[Node("On User Joined", "Events")]
public sealed class OnUserJoinedNode : Node, INodeEventHandler
{
    public FlowCall OnUserJoined = new();

    public ValueOutput<User> User = new("User");

    protected override async Task Process(PulseContext c)
    {
        await OnUserJoined.Execute(c);
    }

    public Task<bool> HandleOnUserJoined(PulseContext c, VRChatClientEventUserJoined eventArgs)
    {
        User.Write(eventArgs.User, c);
        return Task.FromResult(true);
    }
}

[Node("On User Left", "Events")]
public sealed class OnUserLeftNode : Node, INodeEventHandler
{
    public FlowCall OnUserLeft = new();

    public ValueOutput<User> User = new("User");

    protected override async Task Process(PulseContext c)
    {
        await OnUserLeft.Execute(c);
    }

    public Task<bool> HandleOnUserLeft(PulseContext c, VRChatClientEventUserLeft eventArgs)
    {
        User.Write(eventArgs.User, c);
        return Task.FromResult(true);
    }
}

[Node("On Avatar PreChange", "Events")]
public sealed class OnAvatarPreChangeNode : Node, INodeEventHandler
{
    public FlowCall OnAvatarPreChange = new();

    protected override async Task Process(PulseContext c)
    {
        await OnAvatarPreChange.Execute(c);
    }

    public Task<bool> HandleOnAvatarPreChange(PulseContext c, VRChatClientEventAvatarPreChange eventArgs) => Task.FromResult(true);
}