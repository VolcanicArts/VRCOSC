// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using FastOSC;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.SDK.VRChat;

public record Avatar
{
    public readonly string Id;
    public readonly string Name;
    public readonly IReadOnlyList<ParameterDefinition> Parameters;

    public float EyeHeight { get; internal set; }
    public float EyeHeightMin { get; internal set; }
    public float EyeHeightMax { get; internal set; }
    public bool EyeHeightScalingAllowed { get; internal set; }

    internal Avatar(string id, string name, ParameterDefinition[] parameters)
    {
        Id = id;
        Name = name;
        Parameters = parameters;
    }

    public override string ToString() => $"{Id} ({Name})";

    public override int GetHashCode() => Id.GetHashCode();

    public virtual bool Equals(Avatar? other) => Id == other?.Id;

    internal void HandleOSCMessage(OSCMessage message)
    {
        if (message.Address == VRChatOSCConstants.ADDRESS_AVATAR_EYEHEIGHT)
            EyeHeight = (float)message.Arguments[0]!;

        if (message.Address == $"{VRChatOSCConstants.ADDRESS_AVATAR_EYEHEIGHT}min")
            EyeHeightMin = (float)message.Arguments[0]!;

        if (message.Address == $"{VRChatOSCConstants.ADDRESS_AVATAR_EYEHEIGHT}max")
            EyeHeightMax = (float)message.Arguments[0]!;

        if (message.Address == $"{VRChatOSCConstants.ADDRESS_AVATAR_EYEHEIGHT}scalingallowed")
            EyeHeightScalingAllowed = (bool)message.Arguments[0]!;
    }

    public void SetEyeHeight(float meters)
    {
        if (!EyeHeightScalingAllowed) return;

        AppManager.GetInstance().VRChatOscClient.Send(VRChatOSCConstants.ADDRESS_AVATAR_EYEHEIGHT, meters);
    }
}