// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using VRCOSC.Game.OSC.Client;

namespace VRCOSC.Game.OSC.VRChat;

public class VRChatOscClient : OscClient
{
    public Action<VRChatOscData>? OnParameterSent;
    public Action<VRChatOscData>? OnParameterReceived;

    public VRChatOscClient()
    {
        OnRawDataReceived += byteData =>
        {
            var message = OscDecoder.Decode(byteData);
            var data = new VRChatOscData(new OscData(message.Address, message.Values));

            if (!data.Values.Any()) return;

            OnParameterReceived?.Invoke(new VRChatOscData(data));
        };
    }

    public void SendValue(string address, object value) => SendValues(address, new List<object> { value });
    public void SendValues(string address, List<object> values) => SendData(new OscData(address, values));

    public void SendData(OscData data)
    {
        data.PreValidate();
        SendByteData(data.Encode());
        OnParameterSent?.Invoke(new VRChatOscData(data));
    }
}
