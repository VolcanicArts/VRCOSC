// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.OSC.Client;

internal static class OscExtensions
{
    public static int FirstIndexAfter<T>(this IEnumerable<T> items, int start, Func<T, bool> predicate)
    {
        var itemsList = items.ToList();

        if (start >= itemsList.Count) throw new ArgumentOutOfRangeException(nameof(start));

        var retVal = 0;

        foreach (var item in itemsList)
        {
            if (retVal >= start && predicate(item)) return retVal;

            retVal++;
        }

        return -1;
    }

    public static T[] SubArray<T>(this T[] data, int index, int length)
    {
        var result = new T[length];
        Array.Copy(data, index, result, 0, length);
        return result;
    }
}
