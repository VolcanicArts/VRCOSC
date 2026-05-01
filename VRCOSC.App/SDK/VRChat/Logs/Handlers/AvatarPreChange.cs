// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Text.RegularExpressions;

namespace VRCOSC.App.SDK.VRChat.Logs.Handlers;

public class AvatarChangeStartLogLineHandler : VRChatLogLineHandler
{
    public override Regex Regex => new(@"^.+Initialize Limb Avatar VRCPlayer\[Local\].+$");

    public override IVRChatClientEvent HandleMatch(LogReaderState state, VRChatLogLineMatch logLine) => new AvatarPreChangeClientEvent(logLine.Timestamp);
}

public class AvatarPreChangeClientEvent : VRChatClientEvent
{
    public AvatarPreChangeClientEvent(DateTime timestamp)
        : base(timestamp)
    {
    }
}