// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text;

namespace VRCOSC.OSC.Client;

public sealed class OscMessage : OscPacket
{
    public readonly string Address;
    public readonly List<object> Values;

    public OscMessage(string address, List<object> values)
    {
        if (address.Length == 0) throw new InvalidOperationException($"{nameof(address)} must have a non-zero length");

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
                case int intArg:
                    typeStringBuilder.Append('i');
                    parts.Add(SetInt(intArg));
                    break;

                case float floatArg:
                    typeStringBuilder.Append('f');
                    parts.Add(SetFloat(floatArg));
                    break;

                case string stringArg:
                    typeStringBuilder.Append('s');
                    parts.Add(SetString(stringArg));
                    break;

                case bool boolArg:
                    typeStringBuilder.Append(boolArg ? 'T' : 'F');
                    break;

                default:
                    throw new InvalidOperationException($"Cannot parse type {value.GetType()}");
            }
        }

        var typeString = typeStringBuilder.ToString();

        var addressLen = (Encoding.UTF8.GetBytes(Address).Length / 4 + 1) * 4;
        var typeLen = (Encoding.UTF8.GetBytes(typeString).Length / 4 + 1) * 4;

        var total = addressLen + typeLen + parts.Sum(x => x.Length);

        var output = new byte[total];
        var index = 0;

        Encoding.UTF8.GetBytes(Address).CopyTo(output, index);
        index += addressLen;

        Encoding.UTF8.GetBytes(typeString).CopyTo(output, index);
        index += typeLen;

        foreach (var part in parts)
        {
            part.CopyTo(output, index);
            index += part.Length;
        }

        return output;
    }
}
