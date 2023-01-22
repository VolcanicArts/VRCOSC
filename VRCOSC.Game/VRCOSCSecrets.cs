// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;

namespace VRCOSC.Game;

public static class VRCOSCSecrets
{
    private static readonly Dictionary<Keys, string> secrets = new();

    public static void Init()
    {
    }

    public static string GetKey(Keys key) => secrets.TryGetValue(key, out var value) ? value : string.Empty;

    public enum Keys
    {
        Hyperate,
        Weather
    }
}
