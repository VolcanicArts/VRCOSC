// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Text.RegularExpressions;

namespace VRCOSC.App.SDK.VRChat.Logs.Handlers;

public class InstanceLeftLogLineHandler : VRChatLogLineHandler
{
    public override Regex Regex => new("^.+OnLeftRoom$");

    public override IVRChatClientEvent HandleMatch(LogReaderState state, VRChatLogLineMatch logLine)
    {
        return new InstanceLeftClientEvent(logLine.Timestamp, new Instance(state.InstanceId, state.InstanceOwnerId, state.InstanceNumber, state.InstanceType, state.InstanceRegion, state.InstanceAgeGated, state.InstanceHasQueue, new World(state.WorldId, state.WorldName)));
    }
}

public class InstanceLeftClientEvent : VRChatClientEvent
{
    public readonly Instance Instance;

    public InstanceLeftClientEvent(DateTime timestamp, Instance instance)
        : base(timestamp)
    {
        Instance = instance;
    }
}