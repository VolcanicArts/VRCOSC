// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Bindables;

namespace VRCOSC.Game.ChatBox.Clips;

/// <summary>
/// Represents a timespan that contains all information the ChatBox will need for displaying
/// </summary>
public class Clip
{
    public readonly BindableBool Enabled = new(true);
    public readonly Bindable<string> Name = new("New Clip");
    public readonly BindableNumber<int> Priority = new();
    public readonly BindableList<string> AssociatedModules = new();
    public readonly BindableList<ChatBoxVariable> AvailableVariables = new();
    public readonly Dictionary<string, ClipState> States = new();
    public readonly Dictionary<string, ClipEvent> Events = new();
    public readonly Bindable<float> Start = new();
    public readonly Bindable<float> End = new(0.5f);
    public float Length => End.Value - Start.Value;

    public Clip()
    {
        AssociatedModules.BindCollectionChanged((_, _) => calculateAvailableVariables(), true);
    }

    private void calculateAvailableVariables()
    {
        AvailableVariables.Clear();

        foreach (var module in AssociatedModules)
        {
            //AvailableVariables.AddRange(module.Variables);
        }
    }

    public bool Intersects(Clip other)
    {
        if (other.Start.Value >= Start.Value && other.Start.Value <= End.Value) return true;
        if (other.End.Value >= Start.Value && other.End.Value <= End.Value) return true;
        if (other.Start.Value <= Start.Value && other.End.Value >= End.Value) return true;

        return false;
    }
}
