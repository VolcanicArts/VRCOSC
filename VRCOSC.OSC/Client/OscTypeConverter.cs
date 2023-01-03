// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text;

namespace VRCOSC.OSC.Client;

internal static class OscTypeConverter
{
    internal static int CalculateAlignedLength(IReadOnlyCollection<byte> bytes) => (bytes.Count / 4 + 1) * 4;

    internal static byte[] IntToBytes(int value)
    {
        return BitConverter.GetBytes(value).Reverse().ToArray();
    }

    internal static byte[] FloatToBytes(float value)
    {
        return BitConverter.GetBytes(value).Reverse().ToArray();
    }

    internal static byte[] StringToBytes(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);

        var msg = new byte[CalculateAlignedLength(bytes)];
        bytes.CopyTo(msg, 0);

        return msg;
    }

    internal static int BytesToInt(IReadOnlyList<byte> msg, int index)
    {
        return (msg[index] << 24) + (msg[index + 1] << 16) + (msg[index + 2] << 8) + (msg[index + 3] << 0);
    }

    internal static float BytesToFloat(IReadOnlyList<byte> msg, int index)
    {
        var reversed = new byte[4];
        reversed[3] = msg[index];
        reversed[2] = msg[index + 1];
        reversed[1] = msg[index + 2];
        reversed[0] = msg[index + 3];
        return BitConverter.ToSingle(reversed, 0);
    }

    internal static string BytesToString(byte[] msg, int start)
    {
        var i = start + 4;

        for (; i < msg.Length; i += 4)
        {
            if (msg[i - 1] != 0) continue;

            return Encoding.UTF8.GetString(msg.SubArray(start, i - start));
        }

        throw new InvalidOperationException("No null terminator after type string");
    }
}
