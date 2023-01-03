// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.OSC.Client;

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
        if (!Values.All(value => value is (bool or int or float or string)))
            throw new ArgumentOutOfRangeException(nameof(Values), "Cannot send values that are not of type bool, int, float, or string");
    }
}
