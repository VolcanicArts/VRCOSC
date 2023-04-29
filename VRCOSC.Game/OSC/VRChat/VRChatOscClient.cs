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
    private readonly Dictionary<string, List<object>> valuesCache = new();

    public VRChatOscClient()
    {
        Receiver.OnRawDataReceived += byteData =>
        {
            var message = OscDecoder.Decode(byteData);
            var data = new VRChatOscData(new OscData(message.Address, message.Values));

            if (!data.Values.Any()) return;

            OnParameterReceived?.Invoke(new VRChatOscData(data));
        };
    }

    public void SendValue(string address, object value) => SendValues(address, new List<object> { value });
    public void SendValues(string address, List<object> values) => sendData(new OscData(address, values));

    private void sendData(OscData data)
    {
        var blockSend = false;

        if (valuesCache.TryGetValue(data.Address, out var previousValue))
        {
            if (data.Values.SequenceEqual(previousValue)) blockSend = true;
        }

        valuesCache[data.Address] = data.Values;

        if (blockSend) return;

        data.PreValidate();
        SendByteData(data.Encode());
        OnParameterSent?.Invoke(new VRChatOscData(data));
    }
}
