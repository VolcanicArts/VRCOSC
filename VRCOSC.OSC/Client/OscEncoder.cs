// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text;

namespace VRCOSC.OSC.Client;

internal static class OscEncoder
{
    internal static byte[] Encode(OscMessage message)
    {
        var parts = new List<byte[]>();
        var typeStringBuilder = new StringBuilder(",");

        foreach (var value in message.Values)
        {
            switch (value)
            {
                case int intArg:
                    typeStringBuilder.Append('i');
                    parts.Add(OscTypeConverter.IntToBytes(intArg));
                    break;

                case float floatArg:
                    typeStringBuilder.Append('f');
                    parts.Add(OscTypeConverter.FloatToBytes(floatArg));
                    break;

                case string stringArg:
                    typeStringBuilder.Append('s');
                    parts.Add(OscTypeConverter.StringToBytes(stringArg));
                    break;

                case true:
                    typeStringBuilder.Append('T');
                    break;

                case false:
                    typeStringBuilder.Append('F');
                    break;

                default:
                    throw new InvalidOperationException($"Cannot parse type {value.GetType()}");
            }
        }

        var addressBytes = OscConstants.OSC_ENCODING.GetBytes(message.Address);
        var typeStringBytes = OscConstants.OSC_ENCODING.GetBytes(typeStringBuilder.ToString());

        var addressLen = OscTypeConverter.CalculateAlignedLength(addressBytes);
        var typeLen = OscTypeConverter.CalculateAlignedLength(typeStringBytes);

        var total = addressLen + typeLen + parts.Sum(x => x.Length);

        var output = new byte[total];
        var index = 0;

        addressBytes.CopyTo(output, index);
        index += addressLen;

        typeStringBytes.CopyTo(output, index);
        index += typeLen;

        foreach (var part in parts)
        {
            part.CopyTo(output, index);
            index += part.Length;
        }

        return output;
    }
}
