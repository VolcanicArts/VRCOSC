// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;

namespace VRCOSC.Game.OSC.Client;

public class OscData
{
    public readonly string Address;
    public readonly List<object> Values;

    internal OscData(string address, List<object> values)
    {
        Address = address;
        Values = values;
    }

    internal void PreValidate()
    {
        Values.ForEach(value =>
        {
            if (value is not (bool or int or float or string))
            {
                throw new InvalidOperationException($"Cannot send value that is of type {value.GetType().Name} to address {Address}");
            }
        });
    }

    internal byte[] Encode() => new OscMessage(Address, Values).Encode();
}
