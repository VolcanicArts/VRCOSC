// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osu.Framework.Logging;

// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable SuggestBaseTypeForParameter

namespace VRCOSC.OSC.Client;

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

        if (typeTags is null)
        {
            Logger.Log($"Could not parse type tags for message {Encoding.UTF8.GetString(data)}");
            return null;
        }

        index = OscUtils.AlignIndex(index);

        var values = getValues(typeTags, data, ref index);

        if (values is null || !values.Any())
        {
            Logger.Log($"Could not parse values for message {Encoding.UTF8.GetString(data)}");
            return null;
        }

        return new OscMessage(address, values);
    }

    private static string? getAddress(byte[] data, ref int index)
    {
        var start = index;
        if (data[start] != OscChars.CHAR_SLASH) return null;

        while (data[index] != 0) index++;

        return Encoding.UTF8.GetString(data[start..index]);
    }

    private static byte[]? getTypeTags(byte[] data, ref int index)
    {
        var start = index;
        if (data[start] != OscChars.CHAR_COMMA) return null;

        while (data[index] != 0) index++;

        return data[(start + 1)..index];
    }

    private static List<object>? getValues(byte[] typeTags, byte[] msg, ref int index)
    {
        var values = new List<object>();

        foreach (var type in typeTags)
        {
            switch (type)
            {
                case OscChars.CHAR_INT:
                    values.Add(bytesToInt(msg, ref index));
                    break;

                case OscChars.CHAR_FLOAT:
                    values.Add(bytesToFloat(msg, ref index));
                    break;

                case OscChars.CHAR_STRING:
                    values.Add(bytesToString(msg, ref index));
                    break;

                case OscChars.CHAR_TRUE:
                    values.Add(true);
                    break;

                case OscChars.CHAR_FALSE:
                    values.Add(false);
                    break;

                default:
                    return null;
            }
        }

        return values;
    }

    private static int bytesToInt(byte[] data, ref int index)
    {
        var reversed = new byte[4];
        reversed[3] = data[index++];
        reversed[2] = data[index++];
        reversed[1] = data[index++];
        reversed[0] = data[index++];
        return BitConverter.ToInt32(reversed, 0);
    }

    private static float bytesToFloat(byte[] data, ref int index)
    {
        var reversed = new byte[4];
        reversed[3] = data[index++];
        reversed[2] = data[index++];
        reversed[1] = data[index++];
        reversed[0] = data[index++];
        return BitConverter.ToSingle(reversed, 0);
    }

    private static string bytesToString(byte[] data, ref int index)
    {
        var start = index;
        while (data[index] != 0) index++;

        var stringData = Encoding.UTF8.GetString(data[start..index]);
        index = OscUtils.AlignIndex(index);
        return stringData;
    }
}
