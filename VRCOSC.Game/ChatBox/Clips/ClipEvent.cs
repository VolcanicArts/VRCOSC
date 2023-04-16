// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;

namespace VRCOSC.Game.ChatBox.Clips;

public class ClipEvent
{
    public string Module { get; init; }
    public string Lookup { get; init; }
    public string Name { get; init; }
    public Bindable<string> Format = new();
    public BindableBool Enabled = new();
    public Bindable<int> Length = new();

    public ClipEvent Copy() => new()
    {
        Module = Module,
        Lookup = Lookup,
        Name = Name,
        Format = new Bindable<string>(Format.Value),
        Length = new Bindable<int>(Length.Value)
    };

    public ClipEvent()
    {
    }

    public ClipEvent(ClipEventMetadata metadata)
    {
        Module = metadata.Module;
        Lookup = metadata.Lookup;
        Name = metadata.Name;
        Format.Value = metadata.DefaultFormat;
        Format.Default = metadata.DefaultFormat;
        Length.Value = metadata.DefaultLength;
    }
}

public class ClipEventMetadata
{
    public required string Module { get; init; }
    public required string Lookup { get; init; }
    public required string Name { get; init; }
    public required string DefaultFormat { get; init; }
    public required int DefaultLength { get; init; }
}
