// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

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

    public static async Task<OscMessage> ReceiveOscMessageAsync(this Socket socket, CancellationToken token)
    {
        var receiveResult = new byte[128];
        await socket.ReceiveAsync(receiveResult, SocketFlags.None, token);
        var dWords = bytes_converter.Serialize(receiveResult);
        message_converter.Deserialize(dWords, out var value);
        return value;
    }
}
