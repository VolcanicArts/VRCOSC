// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;

namespace VRCOSC.Game.ChatBox.Clips;

public class ClipState
{
    public required List<(string, string)> States { get; init; }
    public Bindable<string> Format = new();
    public BindableBool Enabled = new();

    public List<string> ModuleNames => States.Select(state => state.Item1).ToList();
    public List<string> StateNames => States.Select(state => state.Item2).ToList();

    public ClipState Copy() => new()
    {
        States = States
    };
}
