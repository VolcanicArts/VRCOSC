// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text;

namespace VRCOSC.OSC;

public abstract class OscPacket
{
    public static OscMessage? ParseMessage(byte[] msg)
    {
        var index = 0;
        var values = new List<object>();

        // Get address
        var address = getAddress(msg, index);
        index += msg.FirstIndexAfter(address.Length, x => x == ',');

        if (index % 4 != 0)
            throw new Exception("Misaligned OSC Packet data. Address string is not padded correctly and does not align to 4 byte interval");

        // Get type tags
        var types = getTypes(msg, index);
        index += types.Length;

        while (index % 4 != 0)
            index++;

        var commaParsed = false;

        foreach (var type in types)
        {
            // skip leading comma
            if (type == ',' && !commaParsed)
            {
                commaParsed = true;
                continue;
            }

            switch (type)
            {
                case '\0':
                    break;

                case 'i':
                    var intVal = getInt(msg, index);
                    values.Add(intVal);
                    index += 4;
                    break;

                case 'f':
                    var floatVal = getFloat(msg, index);
                    values.Add(floatVal);
                    index += 4;
                    break;

                case 's':
                    var stringVal = getString(msg, index);
                    values.Add(stringVal);
                    index += stringVal.Length;
                    break;

                case 'T':
                    values.Add(true);
                    break;

                case 'F':
                    values.Add(false);
                    break;

                default:
                    throw new InvalidOperationException($"OSC type tag '{type}' is unknown.");
            }

            while (index % 4 != 0)
                index++;
        }

        return new OscMessage(address, values);
    }

    private static string getAddress(byte[] msg, int index)
    {
        var i = index;
        var address = string.Empty;

        for (; i < msg.Length; i += 4)
        {
            if (msg[i] == ',')
            {
                if (i == 0) return string.Empty;

                address = Encoding.ASCII.GetString(msg.SubArray(index, i - 1));
                break;
            }
        }

        if (i >= msg.Length && address == null)
            throw new InvalidOperationException("no comma found");

        return address.Replace("\0", string.Empty);
    }

    private static char[] getTypes(byte[] msg, int index)
    {
        var i = index + 4;
        char[]? types = null;

        for (; i < msg.Length; i += 4)
        {
            if (msg[i - 1] == 0)
            {
                types = Encoding.ASCII.GetChars(msg.SubArray(index, i - index));
                break;
            }
        }

        if (i >= msg.Length && types == null)
            throw new InvalidOperationException("No null terminator after type string");

        return types!;
    }

    private static int getInt(IReadOnlyList<byte> msg, int index)
    {
        var val = (msg[index] << 24) + (msg[index + 1] << 16) + (msg[index + 2] << 8) + (msg[index + 3] << 0);
        return val;
    }

    private static float getFloat(IReadOnlyList<byte> msg, int index)
    {
        var reversed = new byte[4];
        reversed[3] = msg[index];
        reversed[2] = msg[index + 1];
        reversed[1] = msg[index + 2];
        reversed[0] = msg[index + 3];
        var val = BitConverter.ToSingle(reversed, 0);
        return val;
    }

    private static string getString(byte[] msg, int index)
    {
        var output = string.Empty;
        var i = index + 4;

        for (; i - 1 < msg.Length; i += 4)
        {
            if (msg[i - 1] == 0)
            {
                output = Encoding.ASCII.GetString(msg.SubArray(index, i - index));
                break;
            }
        }

        if (i >= msg.Length && output == null)
            throw new InvalidOperationException("No null terminator after type string");

        return output.Replace("\0", string.Empty);
    }

    protected static byte[] SetInt(int value)
    {
        var msg = new byte[4];

        var bytes = BitConverter.GetBytes(value);
        msg[0] = bytes[3];
        msg[1] = bytes[2];
        msg[2] = bytes[1];
        msg[3] = bytes[0];

        return msg;
    }

    protected static byte[] SetFloat(float value)
    {
        var msg = new byte[4];

        var bytes = BitConverter.GetBytes(value);
        msg[0] = bytes[3];
        msg[1] = bytes[2];
        msg[2] = bytes[1];
        msg[3] = bytes[0];

        return msg;
    }

    protected static byte[] SetString(string value)
    {
        var len = OscUtils.AlignedStringLength(value);
        var msg = new byte[len];

        var bytes = Encoding.ASCII.GetBytes(value);
        bytes.CopyTo(msg, 0);

        return msg;
    }
}
