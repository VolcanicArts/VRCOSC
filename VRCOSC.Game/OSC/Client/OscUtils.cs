// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.CompilerServices;

namespace VRCOSC.OSC.Client;

public static class OscUtils
{
    /// <summary>
    /// This will force add 4 empty bytes even if the index is aligned to facilitate the specification's required gap
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AlignIndex(int index) => index + (4 - index % 4);
}
