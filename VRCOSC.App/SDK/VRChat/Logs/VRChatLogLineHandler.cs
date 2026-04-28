// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Text.RegularExpressions;

namespace VRCOSC.App.SDK.VRChat.Logs;

public interface IVRChatLogLineHandler
{
    Regex Regex { get; }

    IVRChatClientEvent? HandleMatch(LogReaderState state, VRChatLogLineMatch logLine);
}

public abstract class VRChatLogLineHandler : IVRChatLogLineHandler
{
    public abstract Regex Regex { get; }

    public abstract IVRChatClientEvent? HandleMatch(LogReaderState state, VRChatLogLineMatch logLine);
}

public interface IVRChatClientEvent
{
    DateTime Timestamp { get; }
}

public abstract class VRChatClientEvent : IVRChatClientEvent
{
    public DateTime Timestamp { get; }

    protected VRChatClientEvent(DateTime timestamp)
    {
        Timestamp = timestamp;
    }
}