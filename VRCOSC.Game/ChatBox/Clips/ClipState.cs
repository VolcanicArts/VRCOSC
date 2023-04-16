// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;

namespace VRCOSC.Game.ChatBox.Clips;

public class ClipState
{
    public List<(string, string)> States { get; init; }
    public Bindable<string> Format = new();
    public BindableBool Enabled = new();

    public List<string> ModuleNames => States.Select(state => state.Item1).ToList();
    public List<string> StateNames => States.Select(state => state.Item2).ToList();

    public ClipState Copy()
    {
        var statesCopy = new List<(string, string)>();
        States.ForEach(state => statesCopy.Add(state));

        return new ClipState
        {
            States = statesCopy
        };
    }

    public ClipState()
    {
    }

    public ClipState(ClipStateMetadata metadata)
    {
        States = new List<(string, string)> { (metadata.Module, metadata.Lookup) };
        Format.Value = metadata.DefaultFormat;
        Format.Default = metadata.DefaultFormat;
    }
}

public class ClipStateMetadata
{
    public required string Module { get; init; }
    public required string Lookup { get; init; }
    public required string Name { get; init; }
    public required string DefaultFormat { get; init; }
}
