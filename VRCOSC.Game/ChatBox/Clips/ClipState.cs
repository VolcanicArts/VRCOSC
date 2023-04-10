// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;

namespace VRCOSC.Game.ChatBox.Clips;

public class ClipState
{
    public required string Lookup { get; init; }
    public required string Name { get; init; }
    public Bindable<string> Format = new();
    public BindableBool Enabled = new();
}
