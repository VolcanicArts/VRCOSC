// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;

namespace VRCOSC.Game.OSC.Client;

internal static class OscTypeConverter
{
    public static int CalculateAlignedLength(IReadOnlyCollection<byte> bytes) => (bytes.Count / 4 + 1) * 4;

    public static int IntToBytes(int value, byte[] buffer, int index)
    {
        var bytes = BitConverter.GetBytes(value);

        buffer[index + 3] = bytes[0];
        buffer[index + 2] = bytes[1];
        buffer[index + 1] = bytes[2];
        buffer[index + 0] = bytes[3];

        return 4;
    }

    public static int FloatToBytes(float value, byte[] buffer, int index)
    {
        var bytes = BitConverter.GetBytes(value);

        buffer[index + 3] = bytes[0];
        buffer[index + 2] = bytes[1];
        buffer[index + 1] = bytes[2];
        buffer[index + 0] = bytes[3];

        return 4;
    }

    public static int StringToBytes(string value, byte[] buffer, int index)
    {
        var bytes = OscConstants.OSC_ENCODING.GetBytes(value);
        var length = CalculateAlignedLength(bytes);

        bytes.CopyTo(buffer, index);

        return length;
    }

    public static int BytesToInt(IReadOnlyList<byte> msg, int index) => (msg[index] << 24) + (msg[index + 1] << 16) + (msg[index + 2] << 8) + (msg[index + 3] << 0);

    public static float BytesToFloat(IReadOnlyList<byte> msg, int index)
    {
        var reversed = new byte[4];
        reversed[3] = msg[index];
        reversed[2] = msg[index + 1];
        reversed[1] = msg[index + 2];
        reversed[0] = msg[index + 3];
        return BitConverter.ToSingle(reversed, 0);
    }

    public static string BytesToString(byte[] msg, int index)
    {
        for (var i = index + 4; i < msg.Length; i += 4)
        {
            if (msg[i - 1] == 0) return OscConstants.OSC_ENCODING.GetString(msg.SubArray(index, i - index));
        }

        throw new InvalidOperationException("No null terminator after type string");
    }
}
