// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using System.Net.Sockets;
using CoreOSC;
using CoreOSC.Types;

namespace VRCOSC.Game.Modules;

public static class SocketExtensions
{
    private static readonly BytesConverter bytes_converter = new();
    private static readonly OscMessageConverter message_converter = new();

    public static void SendOscMessage(this Socket socket, OscMessage message)
    {
        var dWords = message_converter.Serialize(message);
        _ = bytes_converter.Deserialize(dWords, out var bytes);
        var byteArray = bytes.ToArray();
        socket.Send(byteArray);
    }

    public static OscMessage ReceiveOscMessage(this Socket socket)
    {
        var receiveResult = new byte[128];
        socket.Receive(receiveResult);
        var dWords = bytes_converter.Serialize(receiveResult);
        message_converter.Deserialize(dWords, out var value);
        return value;
    }
}
