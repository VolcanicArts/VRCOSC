// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text;

namespace VRCOSC.OSC.Client;

public sealed class OscMessage
{
    public readonly string Address;
    public readonly List<object> Values;

    public OscMessage(string address, List<object> values)
    {
        if (address.Length == 0) throw new InvalidOperationException($"{nameof(address)} must have a non-zero length");
        if (values.Count == 0) throw new InvalidOperationException($"{nameof(values)} must contain at least one element");

        Address = address;
        Values = values;
    }

    public byte[] GetBytes()
    {
        var parts = new List<byte[]>();
        var typeStringBuilder = new StringBuilder(",");

        foreach (var value in Values)
        {
            switch (value)
            {
                case int intValue:
                    typeStringBuilder.Append('i');
                    parts.Add(OscTypeConverter.IntToBytes(intValue));
                    break;

                case float floatValue:
                    typeStringBuilder.Append('f');
                    parts.Add(OscTypeConverter.FloatToBytes(floatValue));
                    break;

                case string stringValue:
                    typeStringBuilder.Append('s');
                    parts.Add(OscTypeConverter.StringToBytes(stringValue));
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

        var typeString = typeStringBuilder.ToString();

        var addressBytes = Encoding.UTF8.GetBytes(Address);
        var typeBytes = Encoding.UTF8.GetBytes(typeString);

        var output = new byte[OscTypeConverter.CalculateAlignedLength(addressBytes) + OscTypeConverter.CalculateAlignedLength(typeBytes) + parts.Sum(x => x.Length)];
        var index = 0;

        addressBytes.CopyTo(output, index);
        index += OscTypeConverter.CalculateAlignedLength(addressBytes);

        typeBytes.CopyTo(output, index);
        index += OscTypeConverter.CalculateAlignedLength(typeBytes);

        foreach (var part in parts)
        {
            part.CopyTo(output, index);
            index += part.Length;
        }

        return output;
    }
}
