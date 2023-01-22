// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace VRCOSC.Game.OSC.Client;

internal static class OscTypeConverter
{
    public static int CalculateAlignedLength(IReadOnlyCollection<byte> bytes) => (bytes.Count / 4 + 1) * 4;

    public static byte[] IntToBytes(int value) => BitConverter.GetBytes(value).Reverse().ToArray();

    public static byte[] FloatToBytes(float value) => BitConverter.GetBytes(value).Reverse().ToArray();

    public static byte[] StringToBytes(string value)
    {
        var bytes = OscConstants.OSC_ENCODING.GetBytes(value);

        var msg = new byte[CalculateAlignedLength(bytes)];
        bytes.CopyTo(msg, 0);

        return msg;
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
