// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.OSC;

public static class OscUtils
{
    public static int AlignedStringLength(string val)
    {
        var len = val.Length + (4 - val.Length % 4);
        if (len <= val.Length) len += 4;

        return len;
    }
}
