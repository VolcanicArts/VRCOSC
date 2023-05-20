// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.Game.OSC.Client;

namespace VRCOSC.Game.OSC.VRChat;

public class VRChatOscData : OscData
{
    public bool IsAvatarChangeEvent => Address == VRChatOscConstants.ADDRESS_AVATAR_CHANGE;
    public bool IsAvatarParameter => Address.StartsWith(VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX);

    public string ParameterName => Address.Remove(0, VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX.Length + 1);
    public object ParameterValue => Values[0];

    public bool IsValueType(Type type) => ParameterValue.GetType() == type;
    public bool IsValueType<T>() => IsValueType(typeof(T));
    public T ValueAs<T>() => (T)ParameterValue;

    public VRChatOscData(OscData data)
        : base(data.Address, data.Values)
    {
    }
}
