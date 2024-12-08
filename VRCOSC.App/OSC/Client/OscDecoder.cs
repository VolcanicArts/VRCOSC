// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Buffers.Binary;
using System.Text;
using VRCOSC.App.Utils;

namespace VRCOSC.App.OSC.Client;

internal static class OscDecoder
{
    internal static OscMessage? Decode(byte[] data)
    {
        var index = 0;

        var address = getAddress(data, ref index);

        if (address is null)
        {
            Logger.Log($"Could not parse address for message {Encoding.UTF8.GetString(data)}");
            return null;
        }

        index = OscUtils.AlignIndex(index);

        var typeTags = getTypeTags(data, ref index);

        if (typeTags.Length == 0)
        {
            Logger.Log($"Could not parse type tags for message {Encoding.UTF8.GetString(data)}");
            return null;
        }

        index = OscUtils.AlignIndex(index);

        var values = getValues(typeTags, data, ref index);

        return new OscMessage(address, values);
    }

    private static string? getAddress(byte[] data, ref int index)
    {
        var start = index;
        if (data[start] != OscChars.CHAR_SLASH) return null;

        while (data[index] != 0) index++;

        return Encoding.UTF8.GetString(data.AsSpan(start, index - start));
    }

    private static Span<byte> getTypeTags(byte[] data, ref int index)
    {
        var start = index;
        if (data[start] != OscChars.CHAR_COMMA) return Array.Empty<byte>();

        while (data[index] != 0) index++;

        return data.AsSpan((start + 1)..index);
    }

    private static object[] getValues(Span<byte> typeTags, byte[] msg, ref int index)
    {
        var values = new object[typeTags.Length];

        for (var i = 0; i < typeTags.Length; i++)
        {
            var type = typeTags[i];

            values[i] = type switch
            {
                OscChars.CHAR_INT => bytesToInt(msg, ref index),
                OscChars.CHAR_FLOAT => bytesToFloat(msg, ref index),
                OscChars.CHAR_STRING => bytesToString(msg, ref index),
                OscChars.CHAR_TRUE => true,
                OscChars.CHAR_FALSE => false,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        return values;
    }

    private static int bytesToInt(byte[] data, ref int index)
    {
        var value = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(index, 4));
        index += 4;
        return value;
    }

    private static float bytesToFloat(byte[] data, ref int index)
    {
        var value = BinaryPrimitives.ReadSingleBigEndian(data.AsSpan(index, 4));
        index += 4;
        return value;
    }

    private static string bytesToString(byte[] data, ref int index)
    {
        var start = index;
        while (data[index] != 0) index++;

        var stringData = Encoding.UTF8.GetString(data.AsSpan(start, index - start));
        index = OscUtils.AlignIndex(index);
        return stringData;
    }
}
