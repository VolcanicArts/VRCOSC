// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using FastOSC;

namespace VRCOSC.App.OSC.VRChat;

public record VRChatOSCMessage : OSCMessage
{
    public bool IsAvatarChangeEvent => Address == VRChatOSCConstants.ADDRESS_AVATAR_CHANGE;
    public bool IsAvatarParameter => Address.StartsWith(VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX);
    public bool IsChatboxInput => Address == VRChatOSCConstants.ADDRESS_CHATBOX_INPUT;
    public bool IsDollyEvent => Address.StartsWith(VRChatOSCConstants.ADDRESS_DOLLY_PREFIX);
    public bool IsUserCamera => Address.StartsWith(VRChatOSCConstants.ADDRESS_USERCAMERA_PREFIX);

    public string ParameterName
    {
        get
        {
            try
            {
                return field ??= Address[VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX.Length..];
            }
            // This should never be possible if ParameterName is only called after IsAvatarParameter, but sometimes there's a corrupt address?
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }

    public object ParameterValue => Arguments[0]!;

    public VRChatOSCMessage(OSCMessage data)
        : base(data.Address, data.Arguments)
    {
    }
}