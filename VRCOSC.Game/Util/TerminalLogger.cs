// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;

namespace VRCOSC.Game.Util;

internal sealed class TerminalLogger
{
    private readonly string name;

    internal TerminalLogger(string name)
    {
        this.name = name;
    }

    internal void Log(string message)
    {
        message.Split('\n').ForEach(msg => Logger.Log($"[{name}]: {msg}", "terminal"));
    }
}
