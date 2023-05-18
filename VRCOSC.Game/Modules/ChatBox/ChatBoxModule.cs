// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.Game.Modules.ChatBox;

public abstract class ChatBoxModule : Module
{
    protected void CreateVariable(Enum lookup, string name, string format) => ChatBoxManager.RegisterVariable(SerialisedName, lookup.ToLookup(), name, format);
    protected void CreateState(Enum lookup, string name, string defaultFormat) => ChatBoxManager.RegisterState(SerialisedName, lookup.ToLookup(), name, defaultFormat);
    protected void CreateEvent(Enum lookup, string name, string defaultFormat, int defaultLength) => ChatBoxManager.RegisterEvent(SerialisedName, lookup.ToLookup(), name, defaultFormat, defaultLength);

    protected void SetVariableValue(Enum lookup, string? value, string suffix = "") => ChatBoxManager.SetVariable(SerialisedName, lookup.ToLookup(), value, suffix);
    protected string GetVariableFormat(Enum lookup) => ChatBoxManager.VariableMetadata[SerialisedName][lookup.ToLookup()].DisplayableFormat;

    protected void SetAllVariableValues<T>(string? value) where T : Enum => setAllVariableValues(typeof(T), value);

    private void setAllVariableValues(Type lookupType, string? value)
    {
        foreach (Enum lookup in Enum.GetValues(lookupType))
        {
            SetVariableValue(lookup, value);
        }
    }

    protected void ChangeStateTo(Enum lookup) => ChatBoxManager.ChangeStateTo(SerialisedName, lookup.ToLookup());
    protected void TriggerEvent(Enum lookup) => ChatBoxManager.TriggerEvent(SerialisedName, lookup.ToLookup());

    protected void SetChatBoxTyping(bool typing) => ChatBoxManager.SetTyping(typing);
}
