// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

// ReSharper disable MemberCanBePrivate.Global

using System;

namespace VRCOSC.Game.OSC.VRChat;

public static class VRChatOscConstants
{
    public const string ADDRESS_AVATAR_PARAMETERS_PREFIX = @"/avatar/parameters";
    public const string ADDRESS_AVATAR_CHANGE = @"/avatar/change";
    public const string ADDRESS_CHATBOX_INPUT = @"/chatbox/input";
    public const string ADDRESS_CHATBOX_TYPING = @"/chatbox/typing";

    public const int UPDATE_FREQUENCY = 20;
    public const int UPDATE_DELTA = (int)(1f / UPDATE_FREQUENCY * 1000f);
    public static readonly TimeSpan UPDATE_TIME_SPAN = TimeSpan.FromMilliseconds(UPDATE_DELTA);
}
