// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.ChatBox.Clips;

/// <summary>
/// Used by modules to denote a provided variable
/// </summary>
public class ClipVariableMetadata
{
    private const string variable_start_char = "{";
    private const string variable_end_char = "}";

    public required string Module { get; init; }
    public required string Lookup { get; init; }
    public required string Name { get; init; }
    public required string Format { get; init; }

    public string DisplayableFormat => $"{variable_start_char}{Module.ToLowerInvariant().Replace("module", string.Empty)}.{Format}{variable_end_char}";
}
