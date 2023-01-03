// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text;

namespace VRCOSC.OSC.Client;

public static class OscDecoder
{
    public static OscMessage Decode(byte[] msg)
    {
        var index = 0;
        var values = new List<object>();

        var address = getAddress(msg, index, out index);
        if (index.Misaligned()) throw new InvalidOperationException("Misaligned OSC packet data after address");

        var types = getTypes(msg, index, out index);
        if (index.Misaligned()) throw new InvalidOperationException("Misaligned OSC packet data after type codes");

        foreach (var type in types)
        {
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
                    var stringVal = OscTypeConverter.BytesToString(msg, index).Replace("\0", string.Empty);
                    values.Add(stringVal);
                    index += Encoding.UTF8.GetBytes(stringVal).Length;
                    break;

                case 'T':
                    values.Add(true);
                    break;

                case 'F':
                    values.Add(false);
                    break;

                default:
                    throw new InvalidOperationException($"OSC type tag '{type}' is unknown");
            }

            while (index % 4 != 0) index++;
        }

        return new OscMessage(address, values);
    }

    private static string getAddress(byte[] msg, int start, out int newPos)
    {
        newPos = start;
        if (start == 0) return string.Empty;

        var addressEnd = msg.FirstIndexAfter(start, x => x == ',');
        if (addressEnd == -1) throw new InvalidOperationException("No comma found when retrieving address");

        newPos += addressEnd;
        return Encoding.UTF8.GetString(msg.SubArray(start, addressEnd - 1)).Replace("\0", string.Empty);
    }

    private static char[] getTypes(byte[] msg, int start, out int newPos)
    {
        newPos = start;

        var typesArray = OscTypeConverter.BytesToString(msg, start).ToArray();

        newPos += msg.FirstIndexAfter(typesArray.Length, x => x == ',');
        return typesArray;
    }
}
