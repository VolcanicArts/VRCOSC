// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Text.RegularExpressions;

namespace VRCOSC.App.SDK.VRChat.Logs.Handlers;

public class InstanceJoinedLogLineHandler : VRChatLogLineHandler
{
    public override Regex Regex => new("^.+Finished entering world.+$");

    public override IVRChatClientEvent HandleMatch(LogReaderState state, VRChatLogLineMatch logLine)
    {
        return new InstanceJoinedClientEvent(logLine.Timestamp, new Instance(state.InstanceId, state.InstanceOwnerId, state.InstanceNumber, state.InstanceType, state.InstanceRegion, state.InstanceAgeGated, state.InstanceHasQueue, new World(state.WorldId, state.WorldName)));
    }
}

public class InstanceJoinedClientEvent : VRChatClientEvent
{
    public readonly Instance Instance;

    public InstanceJoinedClientEvent(DateTime timestamp, Instance instance)
        : base(timestamp)
    {
        Instance = instance;
    }
}