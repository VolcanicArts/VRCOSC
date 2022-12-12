// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

// ReSharper disable MemberCanBePrivate.Global

namespace VRCOSC.Game;

public static class Constants
{
    public const string OSC_ADDRESS_AVATAR_PARAMETERS_PREFIX = @"/avatar/parameters";
    public const string OSC_ADDRESS_AVATAR_CHANGE = @"/avatar/change";
    public const string OSC_ADDRESS_CHATBOX_INPUT = @"/chatbox/input";
    public const string OSC_ADDRESS_CHATBOX_TYPING = @"/chatbox/typing";

    public const int OSC_UPDATE_FREQUENCY = 20;
    public const int OSC_UPDATE_DELTA = (int)(1f / OSC_UPDATE_FREQUENCY * 1000f);
}
