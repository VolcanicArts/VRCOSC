﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.SDK.VRChat;

namespace VRCOSC.App.Nodes.Types.Events;

[Node("On Start", "Events")]
public class OnStartNode : Node, INodeEventHandler
{
    public FlowCall OnStart = new("On Start");

    protected override async Task Process(PulseContext c)
    {
        await OnStart.Execute(c);
    }

    public bool HandleNodeStart(PulseContext c) => true;
}

[Node("On Stop", "Events")]
public class OnStopNode : Node, INodeEventHandler
{
    public FlowCall OnStop = new("On Stop");

    protected override async Task Process(PulseContext c)
    {
        await OnStop.Execute(c);
    }

    public bool HandleNodeStop(PulseContext c) => true;
}

[Node("On Instance Joined", "Events")]
public class OnInstanceJoinedNode : Node, INodeEventHandler
{
    public FlowCall OnInstanceJoined = new();

    protected override async Task Process(PulseContext c)
    {
        await OnInstanceJoined.Execute(c);
    }

    public bool HandleOnInstanceJoined(PulseContext c, VRChatClientEventInstanceJoined eventArgs) => true;
}

[Node("On Instance Left", "Events")]
public class OnInstanceLeftNode : Node, INodeEventHandler
{
    public FlowCall OnInstanceLeft = new();

    protected override async Task Process(PulseContext c)
    {
        await OnInstanceLeft.Execute(c);
    }

    public bool HandleOnInstanceLeft(PulseContext c, VRChatClientEventInstanceLeft eventArgs) => true;
}

[Node("On User Joined", "Events")]
public class OnUserJoinedNode : Node, INodeEventHandler
{
    public FlowCall OnUserJoined = new();

    protected override async Task Process(PulseContext c)
    {
        await OnUserJoined.Execute(c);
    }

    public bool HandleOnUserJoined(PulseContext c, VRChatClientEventUserJoined eventArgs) => true;
}

[Node("On User Left", "Events")]
public class OnUserLeftNode : Node, INodeEventHandler
{
    public FlowCall OnUserLeft = new();

    protected override async Task Process(PulseContext c)
    {
        await OnUserLeft.Execute(c);
    }

    public bool HandleOnUserLeft(PulseContext c, VRChatClientEventUserLeft eventArgs) => true;
}

[Node("On Avatar PreChange", "Events")]
public class OnAvatarPreChangeNode : Node, INodeEventHandler
{
    public FlowCall OnAvatarPreChange = new();

    protected override async Task Process(PulseContext c)
    {
        await OnAvatarPreChange.Execute(c);
    }

    public bool HandleOnAvatarPreChange(PulseContext c, VRChatClientEventAvatarPreChange eventArgs) => true;
}