﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using FastOSC;

namespace VRCOSC.App.OSC.VRChat;

public record VRChatOscMessage : OSCMessage
{
    public bool IsAvatarChangeEvent => Address == VRChatOscConstants.ADDRESS_AVATAR_CHANGE;
    public bool IsAvatarParameter => Address.StartsWith(VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX);
    public bool IsChatboxInput => Address == VRChatOscConstants.ADDRESS_CHATBOX_INPUT;
    public bool IsDollyEvent => Address.StartsWith(VRChatOscConstants.ADDRESS_DOLLY_PREFIX);

    private string? parameterName;

    public string ParameterName
    {
        get
        {
            try
            {
                return parameterName ??= Address[VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX.Length..];
            }
            // This should never be possible if ParameterName is only called after IsAvatarParameter, but sometimes there's a corrupt address?
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }

    public object ParameterValue => Arguments[0]!;

    public VRChatOscMessage(OSCMessage data)
        : base(data.Address, data.Arguments)
    {
    }
}