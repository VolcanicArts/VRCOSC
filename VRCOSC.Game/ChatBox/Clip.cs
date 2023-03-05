// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;

namespace VRCOSC.Game.ChatBox;

/// <summary>
/// Represents a timespan that contains all information the ChatBox will need for displaying
/// </summary>
public class Clip
{
    public readonly BindableBool Enabled = new(true);
    public readonly Bindable<string> Name = new("New Clip");
    public readonly List<string> AssociatedModules = new();
    public readonly Dictionary<string, ClipState> States = new();
    public readonly Dictionary<string, ClipEvent> Events = new();
    public readonly Bindable<TimeSpan> Start = new();
    public readonly Bindable<TimeSpan> End = new();

    public TimeSpan Length => End.Value - Start.Value;
}
