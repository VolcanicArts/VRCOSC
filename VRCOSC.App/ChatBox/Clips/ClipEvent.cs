﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox.Clips;

public class ClipEvent : ClipElement
{
    public string ModuleID { get; } = null!;
    public string EventID { get; } = null!;

    public Observable<float> Length = new();

    public override bool IsDefault => base.IsDefault && Length.IsDefault;

    [JsonConstructor]
    public ClipEvent()
    {
    }

    public ClipEvent(ModuleChatBoxEvent reference)
    {
        ModuleID = reference.ModuleID;
        EventID = reference.EventID;
        Format = new Observable<string>(reference.DefaultFormat);
        Length = new Observable<float>(reference.DefaultLength);
    }
}

/// <summary>
/// Used as a reference for what events a module has created.
/// </summary>
public class ModuleChatBoxEvent
{
    public required string ModuleID { get; init; }
    public required string EventID { get; init; }
    public required string DefaultFormat { get; init; }
    public required float DefaultLength { get; init; }

    public Observable<string> DisplayName { get; } = new("INVALID");
}
