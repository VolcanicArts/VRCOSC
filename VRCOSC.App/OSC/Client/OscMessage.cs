// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.OSC.Client;

public class OscMessage
{
    public readonly string Address;
    public readonly object[] Values;

    public OscMessage(string address, object[] values)
    {
        if (address.Length == 0) throw new InvalidOperationException($"{nameof(address)} must have a non-zero length");
        if (values.Length == 0) throw new InvalidOperationException($"{nameof(values)} must have a non-zero length");

        Address = address;
        Values = values;
    }
}
