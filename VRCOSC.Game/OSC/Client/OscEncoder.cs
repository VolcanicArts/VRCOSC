// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Text;

namespace VRCOSC.Game.OSC.Client;

internal static class OscEncoder
{
    private static readonly byte[] data_buffer = new byte[4096];
    private static readonly StringBuilder type_buffer = new();

    internal static byte[] Encode(OscMessage message)
    {
        data_buffer.Initialize();
        type_buffer.Clear().Append(',');

        var index = 0;

        foreach (var value in message.Values)
        {
            switch (value)
            {
                case int intArg:
                    type_buffer.Append('i');
                    index += OscTypeConverter.IntToBytes(intArg, data_buffer, index);
                    break;

                case float floatArg:
                    type_buffer.Append('f');
                    index += OscTypeConverter.FloatToBytes(floatArg, data_buffer, index);
                    break;

                case string stringArg:
                    type_buffer.Append('s');
                    index += OscTypeConverter.StringToBytes(stringArg, data_buffer, index);
                    break;

                case true:
                    type_buffer.Append('T');
                    break;

                case false:
                    type_buffer.Append('F');
                    break;

                default:
                    throw new InvalidOperationException($"Cannot parse type {value.GetType()}");
            }
        }

        var addressBytes = OscConstants.OSC_ENCODING.GetBytes(message.Address);
        var typeStringBytes = OscConstants.OSC_ENCODING.GetBytes(type_buffer.ToString());

        var addressLen = OscTypeConverter.CalculateAlignedLength(addressBytes);
        var typeLen = OscTypeConverter.CalculateAlignedLength(typeStringBytes);

        var truncatedBuffer = data_buffer.SubArray(0, index);
        var total = addressLen + typeLen + truncatedBuffer.Length;

        var output = new byte[total];
        index = 0;

        addressBytes.CopyTo(output, index);
        index += addressLen;

        typeStringBytes.CopyTo(output, index);
        index += typeLen;

        truncatedBuffer.CopyTo(output, index);

        return output;
    }
}
