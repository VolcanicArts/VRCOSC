// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using VRCOSC.App.ChatBox;
using VRCOSC.App.ChatBox.Clips;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.ChatBox.Clips.Variables.Instances;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules;

public class ChatBoxModule : AvatarModule
{
    #region Runtime

    protected void ChangeState(Enum lookup)
    {
        ChangeState(lookup.ToLookup());
    }

    protected void ChangeState(string lookup)
    {
        if (GetState(lookup) is null) throw new InvalidOperationException($"State with lookup {lookup} does not exist");

        ChatBoxManager.GetInstance().ChangeStateTo(FullID, lookup);
    }

    protected void TriggerEvent(Enum lookup)
    {
        TriggerEvent(lookup.ToLookup());
    }

    protected void TriggerEvent(string lookup)
    {
        if (GetEvent(lookup) is null) throw new InvalidOperationException($"Event with lookup {lookup} does not exist");

        ChatBoxManager.GetInstance().TriggerEvent(FullID, lookup);
    }

    protected void SetVariableValue<T>(Enum lookup, T value)
    {
        SetVariableValue(lookup.ToLookup(), value);
    }

    protected void SetVariableValue<T>(string lookup, T value)
    {
        var variable = GetVariable(lookup);
        if (variable is null) throw new InvalidOperationException($"Variable with lookup {lookup} does not exist");

        variable.SetValue(value);
    }

    #endregion

    #region States

    protected ClipStateReference? CreateState(Enum lookup, string displayName, string defaultFormat = "", IEnumerable<ClipVariableReference>? defaultVariables = null, bool defaultShowTyping = false)
    {
        return CreateState(lookup.ToLookup(), displayName, defaultFormat, defaultVariables, defaultShowTyping);
    }

    protected ClipStateReference? CreateState(string lookup, string displayName, string defaultFormat = "", IEnumerable<ClipVariableReference>? defaultVariables = null, bool defaultShowTyping = false)
    {
        if (GetState(lookup) is not null)
        {
            ExceptionHandler.Handle($"[{FullID}]: You cannot add the same lookup ({lookup}) for a state more than once");
            return null;
        }

        var clipStateReference = new ClipStateReference
        {
            ModuleID = FullID,
            StateID = lookup,
            DefaultFormat = defaultFormat,
            DefaultShowTyping = defaultShowTyping,
            DefaultVariables = defaultVariables?.ToList() ?? new List<ClipVariableReference>(),
            DisplayName = { Value = displayName }
        };

        ChatBoxManager.GetInstance().CreateState(clipStateReference);
        return clipStateReference;
    }

    protected void DeleteState(Enum lookup)
    {
        DeleteState(lookup.ToLookup());
    }

    protected void DeleteState(string lookup)
    {
        ChatBoxManager.GetInstance().DeleteState(FullID, lookup);
    }

    protected ClipStateReference? GetState(Enum lookup)
    {
        return GetState(lookup.ToLookup());
    }

    protected ClipStateReference? GetState(string lookup)
    {
        return ChatBoxManager.GetInstance().GetState(FullID, lookup);
    }

    #endregion

    #region Events

    protected ClipEventReference? CreateEvent(Enum lookup, string displayName, string defaultFormat = "", IEnumerable<ClipVariableReference>? defaultVariables = null, bool defaultShowTyping = false, float defaultLength = 5, ClipEventBehaviour defaultBehaviour = ClipEventBehaviour.Override)
    {
        return CreateEvent(lookup.ToLookup(), displayName, defaultFormat, defaultVariables, defaultShowTyping, defaultLength, defaultBehaviour);
    }

    protected ClipEventReference? CreateEvent(string lookup, string displayName, string defaultFormat = "", IEnumerable<ClipVariableReference>? defaultVariables = null, bool defaultShowTyping = false, float defaultLength = 5, ClipEventBehaviour defaultBehaviour = ClipEventBehaviour.Override)
    {
        if (GetEvent(lookup) is not null)
        {
            ExceptionHandler.Handle($"[{FullID}]: You cannot add the same lookup ({lookup}) for an event more than once");
            return null;
        }

        var clipEventReference = new ClipEventReference
        {
            ModuleID = FullID,
            EventID = lookup,
            DefaultFormat = defaultFormat,
            DefaultShowTyping = defaultShowTyping,
            DefaultVariables = defaultVariables?.ToList() ?? new List<ClipVariableReference>(),
            DefaultLength = defaultLength,
            DefaultBehaviour = defaultBehaviour,
            DisplayName = { Value = displayName }
        };

        ChatBoxManager.GetInstance().CreateEvent(clipEventReference);
        return clipEventReference;
    }

    protected void DeleteEvent(Enum lookup)
    {
        DeleteEvent(lookup.ToLookup());
    }

    protected void DeleteEvent(string lookup)
    {
        ChatBoxManager.GetInstance().DeleteEvent(FullID, lookup);
    }

    protected ClipEventReference? GetEvent(Enum lookup)
    {
        return GetEvent(lookup.ToLookup());
    }

    protected ClipEventReference? GetEvent(string lookup)
    {
        return ChatBoxManager.GetInstance().GetEvent(FullID, lookup);
    }

    #endregion

    #region Variables

    /// <summary>
    /// Creates a variable using the specified <paramref name="lookup"/>
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <param name="displayName">The display name to show the user</param>
    /// <typeparam name="T">The type of this variable's value</typeparam>
    /// <remarks><paramref name="lookup"/> is turned into a string internally, and is only an enum to allow for easier referencing in your code</remarks>
    protected ClipVariableReference? CreateVariable<T>(Enum lookup, string displayName)
    {
        return CreateVariable<T>(lookup.ToLookup(), displayName);
    }

    /// <summary>
    /// Creates a variable using the specified <paramref name="lookup"/>
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <param name="displayName">The display name to show the user</param>
    /// <typeparam name="T">The type of this variable's value</typeparam>
    protected ClipVariableReference? CreateVariable<T>(string lookup, string displayName)
    {
        Type? clipVariableType = null;

        if (typeof(T) == typeof(bool))
            clipVariableType = typeof(BoolClipVariable);
        else if (typeof(T) == typeof(int))
            clipVariableType = typeof(IntClipVariable);
        else if (typeof(T) == typeof(float))
            clipVariableType = typeof(FloatClipVariable);
        else if (typeof(T) == typeof(string))
            clipVariableType = typeof(StringClipVariable);
        else if (typeof(T) == typeof(DateTimeOffset))
            clipVariableType = typeof(DateTimeClipVariable);
        else if (typeof(T) == typeof(TimeSpan))
            clipVariableType = typeof(TimeSpanClipVariable);

        if (clipVariableType is null)
            throw new InvalidOperationException("No clip variable exists for that type. Request it is added to the SDK or make a custom clip variable");

        return CreateVariable<T>(lookup, displayName, clipVariableType);
    }

    /// <summary>
    /// Creates a variable using the specified <paramref name="lookup"/> and a custom <see cref="ClipVariable"/>
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <param name="displayName">The display name to show the user</param>
    /// <param name="clipVariableType">The type of <see cref="ClipVariable"/> to create when instancing this variable</param>
    /// <typeparam name="T">The type of this variable's value</typeparam>
    /// <remarks><paramref name="lookup"/> is turned into a string internally, and is only an enum to allow for easier referencing in your code</remarks>
    protected ClipVariableReference? CreateVariable<T>(Enum lookup, string displayName, Type clipVariableType)
    {
        return CreateVariable<T>(lookup.ToLookup(), displayName, clipVariableType);
    }

    /// <summary>
    /// Creates a variable using the specified <paramref name="lookup"/> and a custom <see cref="ClipVariable"/>
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <param name="displayName">The display name to show the user</param>
    /// <param name="clipVariableType">The type of <see cref="ClipVariable"/> to create when instancing this variable</param>
    /// <typeparam name="T">The type of this variable's value</typeparam>
    protected ClipVariableReference? CreateVariable<T>(string lookup, string displayName, Type clipVariableType)
    {
        if (GetVariable(lookup) is not null)
        {
            ExceptionHandler.Handle($"[{FullID}]: You cannot add the same lookup ({lookup}) for a variable more than once");
            return null;
        }

        var clipVariableReference = new ClipVariableReference
        {
            ModuleID = FullID,
            VariableID = lookup,
            ClipVariableType = clipVariableType,
            ValueType = typeof(T),
            DisplayName = { Value = displayName }
        };

        ChatBoxManager.GetInstance().CreateVariable(clipVariableReference);
        return clipVariableReference;
    }

    /// <summary>
    /// Allows for deleting a variable at runtime.
    /// This is most useful for when you have variables whose existence is reliant on module settings
    /// and you need to delete the variable when the setting disappears
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <remarks><paramref name="lookup"/> is turned into a string internally, and is only an enum to allow for easier referencing in your code</remarks>
    protected void DeleteVariable(Enum lookup)
    {
        DeleteVariable(lookup.ToLookup());
    }

    /// <summary>
    /// Allows for deleting a variable at runtime.
    /// This is most useful for when you have variables whose existence is reliant on module settings
    /// and you need to delete the variable when the setting disappears
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    protected void DeleteVariable(string lookup)
    {
        ChatBoxManager.GetInstance().DeleteVariable(FullID, lookup);
    }

    /// <summary>
    /// Retrieves the <see cref="ClipVariableReference"/> using the <paramref name="lookup"/> provided
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <remarks><paramref name="lookup"/> is turned into a string internally, and is only an enum to allow for easier referencing in your code</remarks>
    protected ClipVariableReference? GetVariable(Enum lookup)
    {
        return GetVariable(lookup.ToLookup());
    }

    /// <summary>
    /// Retrieves the <see cref="ClipVariableReference"/> using the <paramref name="lookup"/> provided
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    protected ClipVariableReference? GetVariable(string lookup)
    {
        return ChatBoxManager.GetInstance().GetVariable(FullID, lookup);
    }

    #endregion
}
