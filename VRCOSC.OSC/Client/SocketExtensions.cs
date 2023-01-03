// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net.Sockets;

namespace VRCOSC.OSC.Client;

public static class SocketExtensions
{
    private static readonly byte[] buffer = new byte[1024];

    public static void SendOscMessage(this Socket socket, OscMessage message)
    {
        socket.Send(message.GetBytes());
    }

    public static async Task<OscMessage?> ReceiveOscMessageAsync(this Socket socket, CancellationToken token)
    {
        buffer.Initialize();
        await socket.ReceiveAsync(buffer, SocketFlags.None, token);
        return OscDecoder.Decode(buffer);
    }
}
