// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Text.RegularExpressions;

namespace VRCOSC.App.SDK.VRChat.Logs;

public record VRChatLogLineMatch
{
    public readonly DateTime Timestamp;
    public readonly Match Match;

    public VRChatLogLineMatch(DateTime timestamp, Match match)
    {
        Timestamp = timestamp;
        Match = match;
    }
}