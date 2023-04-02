// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.ChatBox;

/// <summary>
/// Used by modules to denote a provided variable
/// </summary>
public class ChatBoxVariable
{
    public required string Lookup { get; init; }
    public required string Format { get; init; }
    public required string Description { get; init; }
}
