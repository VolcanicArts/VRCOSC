// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text.RegularExpressions;

namespace VRCOSC.App.SDK.VRChat.Logs.Handlers;

public class RoomJoinedLogLineHandler : VRChatLogLineHandler
{
    public override Regex Regex => new("^.+Entering Room: (.+)$");

    public override IVRChatClientEvent? HandleMatch(LogReaderState state, VRChatLogLineMatch logLine)
    {
        var worldName = logLine.Match.Groups[1].Captures[0].Value;
        state.WorldName = worldName;
        return null;
    }
}