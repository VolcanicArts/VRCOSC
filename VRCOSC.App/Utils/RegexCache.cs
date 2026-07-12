// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text.RegularExpressions;

namespace VRCOSC.App.Utils;

public class RegexCache
{
    public string Content { get; private set; } = string.Empty;
    private Regex regex { get; set; } = null!;

    public Regex Get(string content)
    {
        if (content == Content) return regex;

        regex = new Regex(content);
        return regex;
    }
}