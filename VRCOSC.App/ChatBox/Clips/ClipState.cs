// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox.Clips;

public class ClipState : ClipElement
{
    // ModuleID, StateID
    public Dictionary<string, string> States { get; } = new();

    [JsonConstructor]
    public ClipState()
    {
    }

    public ClipState(ModuleChatBoxState reference)
    {
        States = new Dictionary<string, string> { { reference.ModuleID, reference.StateID } };
        Format = new Observable<string>(reference.DefaultFormat);
    }

    public ClipState(ClipState original)
    {
        States.AddRange(original.States);
    }

    public ClipState Clone() => new(this);
}

/// <summary>
/// Used as a reference for what states a module has created.
/// </summary>
public class ModuleChatBoxState
{
    public required string ModuleID { get; init; }
    public required string StateID { get; init; }
    public required string DefaultFormat { get; init; }

    public Observable<string> DisplayName { get; } = new("INVALID");
}
