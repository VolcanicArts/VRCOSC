// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.Game.Modules.ChatBox;

public abstract class ChatBoxModule : Module
{
    protected void CreateVariable(Enum lookup, string name, string format)
    {
        ChatBoxManager.RegisterVariable(SerialisedName + "-" + lookup.ToLookup(), name, format);
    }

    protected void SetVariableValue(Enum lookup, string? value)
    {
        ChatBoxManager.SetVariable(SerialisedName + "-" + lookup.ToLookup(), value);
    }

    protected void CreateState(Enum lookup, string name, string defaultFormat)
    {

    }

    protected void ChangeStateTo(Enum lookup)
    {

    }
}
