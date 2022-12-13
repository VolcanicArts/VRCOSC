// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.OSC.Client;

public class OscData
{
    public string Address { get; init; } = string.Empty;
    public List<object> Values { get; init; } = Array.Empty<object>().ToList();

    public void PreValidate()
    {
        if (!Values.All(value => value is (bool or int or float or string)))
            throw new ArgumentOutOfRangeException(nameof(Values), "Cannot send values that are not of type bool, int, float, or string");
    }
}
