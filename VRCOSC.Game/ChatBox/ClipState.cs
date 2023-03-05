// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;

namespace VRCOSC.Game.ChatBox;

public class ClipState
{
    public string Format = string.Empty;
    public BindableBool Enabled = new();
}
