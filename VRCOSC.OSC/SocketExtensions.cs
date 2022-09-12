using System.Net.Sockets;

namespace VRCOSC.OSC;

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
        return OscPacket.ParseMessage(buffer);
    }
}
