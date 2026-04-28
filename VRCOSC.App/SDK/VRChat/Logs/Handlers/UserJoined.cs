// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Text.RegularExpressions;

namespace VRCOSC.App.SDK.VRChat.Logs.Handlers;

public class UserJoinedLogLineHandler : VRChatLogLineHandler
{
    public override Regex Regex => new(@"^.+OnPlayerJoined (.+) \((.+)\)$");

    public override IVRChatClientEvent HandleMatch(LogReaderState state, VRChatLogLineMatch logLine)
    {
        var username = logLine.Match.Groups[1].Captures[0].Value;
        var userId = logLine.Match.Groups[2].Captures[0].Value;

        return new UserJoinedClientEvent(logLine.Timestamp, new User(userId, username));
    }
}

public class UserJoinedClientEvent : VRChatClientEvent
{
    public readonly User User;

    public UserJoinedClientEvent(DateTime timestamp, User user)
        : base(timestamp)
    {
        User = user;
    }
}