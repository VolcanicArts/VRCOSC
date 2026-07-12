// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text.RegularExpressions;

namespace VRCOSC.App.OSC;

public static class OSCPatterns
{
    public static string ToPattern(string str) => $"^(?:{Regex.Escape(str).Replace(@"\*", @"(\S*?)")})$";
}