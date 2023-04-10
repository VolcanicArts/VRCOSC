// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.ChatBox.Clips;

/// <summary>
/// Used by modules to denote a provided variable
/// </summary>
public class ClipVariable
{
    public required string Module { get; init; }
    public required string Lookup { get; init; }
    public required string Name { get; init; }
    public required string Format { get; init; }

    public ClipVariable Copy() => new()
    {
        Module = Module,
        Lookup = Lookup,
        Name = Name,
        Format = Format
    };
}
