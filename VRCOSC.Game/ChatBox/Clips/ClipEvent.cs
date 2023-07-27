// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;

namespace VRCOSC.Game.ChatBox.Clips;

public class ClipEvent : IProvidesFormat
{
    public readonly string Name;

    public string Module { get; init; } = null!;
    public string Lookup { get; init; } = null!;

    public Bindable<string> Format = new()
    {
        Default = string.Empty,
        Value = string.Empty
    };

    public Bindable<bool> Enabled = new();
    public Bindable<float> Length = new();

    public bool IsDefault => Format.IsDefault && Enabled.IsDefault && Length.IsDefault;

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
        Length.Default = metadata.DefaultLength;
    }

    public string GetFormat() => Format.Value;
}

public class ClipEventMetadata
{
    public required string Module { get; init; }
    public required string Lookup { get; init; }
    public required string Name { get; init; }
    public required string DefaultFormat { get; init; }
    public required float DefaultLength { get; init; }
}
