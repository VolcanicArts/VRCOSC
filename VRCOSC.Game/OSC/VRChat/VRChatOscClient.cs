// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using VRCOSC.Game.OSC.Client;

namespace VRCOSC.Game.OSC.VRChat;

public class VRChatOscClient : OscClient
{
    public Action<VRChatOscMessage>? OnParameterSent;
    public Action<VRChatOscMessage>? OnParameterReceived;

    public VRChatOscClient()
    {
        OnMessageSent += message =>
        {
            OnParameterSent?.Invoke(new VRChatOscMessage(message));
        };

        OnMessageReceived += message =>
        {
            var data = new VRChatOscMessage(message);
            if (!data.Values.Any()) return;

            OnParameterReceived?.Invoke(data);
        };
    }
}
