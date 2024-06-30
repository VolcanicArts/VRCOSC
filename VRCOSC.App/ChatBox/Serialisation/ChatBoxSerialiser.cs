// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
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

    public ChatBoxSerialiser(Storage storage, ChatBoxManager reference, Observable<Profile> activeProfile)
        : base(storage, reference, activeProfile)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableChatBox data)
    {
        Reference.Timeline.Length.Value = data.Timeline.Length;

        Reference.Timeline.Clips.AddRange(data.Timeline.Clips.Select(serialisableClip =>
        {
            var clip = new Clip
            {
                Layer = { Value = serialisableClip.Layer },
                Enabled = { Value = serialisableClip.Enabled },
                Name = { Value = serialisableClip.Name },
                Start = { Value = serialisableClip.Start },
                End = { Value = serialisableClip.End }
            };

            clip.LinkedModules.AddRange(serialisableClip.LinkedModules);

            serialisableClip.States.ForEach(serialisableState =>
            {
                var clipState = serialisableState.States is null ? clip.States.FirstOrDefault(clipState => clipState.IsBuiltIn) : clip.States.FirstOrDefault(clipState => clipState.States.SequenceEqual(serialisableState.States));
                if (clipState is null) return;

                clipState.Format.Value = serialisableState.Format;
                clipState.Enabled.Value = serialisableState.Enabled;
                clipState.ShowTyping.Value = serialisableState.ShowTyping;
                clipState.UseMinimalBackground.Value = serialisableState.UseMinimalBackground;

                clipState.Variables.Clear();

                serialisableState.Variables.ForEach(serialisableClipVariable =>
                {
                    var clipVariableReference = Reference.GetVariable(serialisableClipVariable.ModuleID, serialisableClipVariable.VariableID);
                    if (clipVariableReference is null) return;

                    var clipVariable = clipVariableReference.CreateInstance();
                    var optionAttributes = getVariableOptionAttributes(clipVariable.GetType());

                    foreach (var pair in serialisableClipVariable.Options)
                    {
                        var propertyInfo = optionAttributes.FirstOrDefault(optionAttribute => optionAttribute.Key.SerialisedName == pair.Key).Value;
                        if (propertyInfo is null) continue;

                        try
                        {
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
                        catch (Exception)
                        {
                            // if we can't convert the type, just set it to default
                        }
                    }

                    clipState.Variables.Add(clipVariable);
                });
            });

            serialisableClip.Events.ForEach(serialisableEvent =>
            {
                var clipEvent = clip.Events.FirstOrDefault(clipEvent => clipEvent.ModuleID == serialisableEvent.ModuleID && clipEvent.EventID == serialisableEvent.EventID);
                if (clipEvent is null) return;

                clipEvent.Format.Value = serialisableEvent.Format;
                clipEvent.Enabled.Value = serialisableEvent.Enabled;
                clipEvent.ShowTyping.Value = serialisableEvent.ShowTyping;
                clipEvent.UseMinimalBackground.Value = serialisableEvent.UseMinimalBackground;
                clipEvent.Length.Value = serialisableEvent.Length;
                clipEvent.Behaviour.Value = serialisableEvent.Behaviour;

                clipEvent.Variables.Clear();

                serialisableEvent.Variables.ForEach(serialisableClipVariable =>
                {
                    var clipVariableReference = Reference.GetVariable(serialisableClipVariable.ModuleID, serialisableClipVariable.VariableID);
                    if (clipVariableReference is null) return;

                    var clipVariable = clipVariableReference.CreateInstance();
                    var optionAttributes = getVariableOptionAttributes(clipVariable.GetType());

                    foreach (var pair in serialisableClipVariable.Options)
                    {
                        var propertyInfo = optionAttributes.FirstOrDefault(optionAttribute => optionAttribute.Key.SerialisedName == pair.Key).Value;
                        if (propertyInfo is null) continue;

                        try
                        {
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
                        catch (Exception)
                        {
                            // if we can't convert the type, just set it to default
                        }
                    }

                    clipEvent.Variables.Add(clipVariable);
                });
            });

            return clip;
        }));

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
