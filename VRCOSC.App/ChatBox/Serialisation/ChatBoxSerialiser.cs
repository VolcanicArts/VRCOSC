// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using VRCOSC.App.ChatBox.Clips;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.Profiles;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox.Serialisation;

public class ChatBoxSerialiser : ProfiledSerialiser<ChatBoxManager, SerialisableChatBox>
{
    protected override string FileName => "chatbox.json";

    // TODO: Come up with a good system for handling this
    // It's probably best to first check to see if all the deserialised data is valid before writing it to the ChatBoxManager
    // If any data is invalid, prompt the user to fix the broken modules, and in the meantime the ChatBox won't work
    // If the user wants the ChatBox to work without fixing the broken modules, VRCOSC will remove all the invalid data and serialise
    public bool LastDeserialiseHadMissingData { get; private set; }

    public ChatBoxSerialiser(Storage storage, ChatBoxManager reference, Observable<Profile> activeProfile)
        : base(storage, reference, activeProfile)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableChatBox data)
    {
        Reference.Timeline.Length.Value = TimeSpan.FromSeconds(data.Timeline.Length);

        for (var index = 0; index < data.Timeline.Layers.Count; index++)
        {
            var layer = Reference.Timeline.Layers[index];
            var serialisableLayer = data.Timeline.Layers[index];

            layer.Enabled.Value = serialisableLayer.Enabled;
            layer.Clips.AddRange(serialisableLayer.Clips.Select(serialisableClip =>
            {
                var clip = new Clip
                {
                    Enabled = { Value = serialisableClip.Enabled },
                    Name = { Value = serialisableClip.Name },
                    Start = { Value = serialisableClip.Start },
                    End = { Value = serialisableClip.End }
                };

                clip.LinkedModules.AddRange(serialisableClip.LinkedModules);

                clip.States.AddRange(serialisableClip.States.Select(serialisableState =>
                {
                    var clipState = new ClipState
                    {
                        Format = { Value = serialisableState.Format },
                        Enabled = { Value = serialisableState.Enabled }
                    };

                    clipState.States.AddRange(serialisableState.States);
                    clipState.Variables.AddRange(serialisableState.Variables.Select(serialisableClipVariable =>
                    {
                        var clipVariableReference = Reference.VariableReferences.FirstOrDefault(clipVariableReference => clipVariableReference.ModuleID == serialisableClipVariable.ModuleID && clipVariableReference.VariableID == serialisableClipVariable.VariableID);

                        Debug.Assert(clipVariableReference is not null);

                        var clipVariable = (ClipVariable)Activator.CreateInstance(clipVariableReference.ClipVariableType, clipVariableReference)!;

                        var optionAttributes = getVariableOptionAttributes(clipVariable.GetType());

                        foreach (var pair in serialisableClipVariable.Options)
                        {
                            var propertyInfo = optionAttributes.FirstOrDefault(optionAttribute => optionAttribute.Key.SerialisedName == pair.Key).Value;

                            if (propertyInfo is null) continue;

                            object value;

                            if (propertyInfo.PropertyType.IsEnum)
                            {
                                value = Enum.ToObject(propertyInfo.PropertyType, pair.Value!);
                            }
                            else
                            {
                                value = Convert.ChangeType(pair.Value, propertyInfo.PropertyType)!;
                            }

                            propertyInfo.SetValue(clipVariable, value);
                        }

                        return clipVariable;
                    }));

                    return clipState;
                }));

                clip.Events.AddRange(serialisableClip.Events.Select(serialisableEvent =>
                {
                    var clipEvent = new ClipEvent
                    {
                        Format = { Value = serialisableEvent.Format },
                        Enabled = { Value = serialisableEvent.Enabled },
                        ModuleID = serialisableEvent.ModuleID,
                        EventID = serialisableEvent.EventID,
                        Length = { Value = serialisableEvent.Length },
                        Behaviour = { Value = serialisableEvent.Behaviour }
                    };

                    clipEvent.Variables.AddRange(serialisableEvent.Variables.Select(serialisableClipVariable =>
                    {
                        var clipVariableReference = Reference.VariableReferences.FirstOrDefault(clipVariableReference => clipVariableReference.ModuleID == serialisableClipVariable.ModuleID && clipVariableReference.VariableID == serialisableClipVariable.VariableID);

                        Debug.Assert(clipVariableReference is not null);

                        var clipVariable = clipVariableReference.CreateInstance();

                        var optionAttributes = getVariableOptionAttributes(clipVariable.GetType());

                        foreach (var pair in serialisableClipVariable.Options)
                        {
                            var propertyInfo = optionAttributes.FirstOrDefault(optionAttribute => optionAttribute.Key.SerialisedName == pair.Key).Value;

                            if (propertyInfo is null) continue;

                            object value;

                            if (propertyInfo.PropertyType.IsEnum)
                            {
                                try
                                {
                                    value = Enum.ToObject(propertyInfo.PropertyType, pair.Value!);
                                }
                                catch (Exception)
                                {
                                    value = Enum.ToObject(propertyInfo.PropertyType, 0);
                                }
                            }
                            else
                            {
                                value = Convert.ChangeType(pair.Value, propertyInfo.PropertyType)!;
                            }

                            propertyInfo.SetValue(clipVariable, value);
                        }

                        return clipVariable;
                    }));

                    return clipEvent;
                }));

                return clip;
            }));
        }

        return false;
    }

    private Dictionary<ClipVariableOptionAttribute, PropertyInfo> getVariableOptionAttributes(Type? type)
    {
        var options = new Dictionary<ClipVariableOptionAttribute, PropertyInfo>();

        if (type is null) return options;

        options.AddRange(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                             .Where(propertyInfo => propertyInfo.GetCustomAttribute<ClipVariableOptionAttribute>() is not null)
                             .Select(propertyInfo => new KeyValuePair<ClipVariableOptionAttribute, PropertyInfo>(propertyInfo.GetCustomAttribute<ClipVariableOptionAttribute>()!, propertyInfo)));

        return options;
    }
}
