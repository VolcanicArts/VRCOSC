// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Logging;

namespace VRCOSC.Game.OSC.Client;

internal static class OscDecoder
{
    internal static OscMessage? Decode(byte[] msg)
    {
        var index = 0;
        var values = new List<object>();

        var address = getAddress(msg, index);
        index += msg.FirstIndexAfter(address.Length, x => x == ',');

        if (index.IsMisaligned())
        {
            Logger.Error(new InvalidOperationException("Misaligned packet. Ignoring data"), "OSCDecoder experienced an issue");
            return null;
        }

        var types = getTypes(msg, index);
        index += types.Length;

        while (index.IsMisaligned()) index++;

        var commaParsed = false;

        foreach (var type in types)
        {
            if (!commaParsed && type == ',')
            {
                commaParsed = true;
                continue;
            }

            switch (type)
            {
                case '\0':
                    break;

                case 'i':
                    var intVal = OscTypeConverter.BytesToInt(msg, index);
                    values.Add(intVal);
                    index += 4;
                    break;

                case 'f':
                    var floatVal = OscTypeConverter.BytesToFloat(msg, index);
                    values.Add(floatVal);
                    index += 4;
                    break;

                case 's':
                    var stringVal = OscTypeConverter.BytesToString(msg, index);
                    values.Add(stringVal);
                    index += OscConstants.OSC_ENCODING.GetBytes(stringVal).Length;
                    break;

                case 'T':
                    values.Add(true);
                    break;

                case 'F':
                    values.Add(false);
                    break;

                default:
                    throw new InvalidOperationException("Unknown tag");
            }

            while (index.IsMisaligned()) index++;
        }

        return new OscMessage(address, values);
    }

    private static string getAddress(byte[] msg, int index)
    {
        var addressEnd = msg.FirstIndexAfter(index, x => x == ',');
        if (addressEnd == -1) throw new InvalidOperationException("No comma found when retrieving address");

        return OscConstants.OSC_ENCODING.GetString(msg.SubArray(index, addressEnd - 1)).Replace("\0", string.Empty);
    }

    private static char[] getTypes(byte[] msg, int index) => OscTypeConverter.BytesToString(msg, index).ToArray();
}
