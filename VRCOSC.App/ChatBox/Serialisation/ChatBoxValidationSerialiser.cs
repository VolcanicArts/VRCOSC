// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using VRCOSC.App.Modules;
using VRCOSC.App.Profiles;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.ChatBox.Serialisation;

public class ChatBoxValidationSerialiser : ProfiledSerialiser<ChatBoxManager, SerialisableChatBox>
{
    protected override string FileName => "chatbox.json";

    public bool IsValid { get; private set; }

    public ChatBoxValidationSerialiser(Storage storage, ChatBoxManager reference, Observable<Profile> activeProfile)
        : base(storage, reference, activeProfile)
    {
    }

    public void Reset()
    {
        IsValid = true;
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableChatBox data)
    {
        foreach (var serialisableLayer in data.Timeline.Layers)
        {
            foreach (var serialisableClip in serialisableLayer.Clips)
            {
                var areAllModulesLoaded = serialisableClip.LinkedModules.All(moduleID => ModuleManager.GetInstance().IsModuleLoaded(moduleID));

                if (!areAllModulesLoaded)
                {
                    IsValid = false;
                    return false;
                }

                foreach (var serialisableClipState in serialisableClip.States)
                {
                    foreach (var serialisableClipStateState in serialisableClipState.States)
                    {
                        var doesStateReferencesExist = Reference.GetState(serialisableClipStateState.Key, serialisableClipStateState.Value) is not null;

                        if (!doesStateReferencesExist)
                        {
                            IsValid = false;
                            return false;
                        }
                    }

                    foreach (var serialisableClipVariable in serialisableClipState.Variables)
                    {
                        var doesClipVariableExist = Reference.GetVariable(serialisableClipVariable.ModuleID, serialisableClipVariable.VariableID) is not null;

                        if (!doesClipVariableExist)
                        {
                            IsValid = false;
                            return false;
                        }
                    }
                }

                foreach (var serialisableClipEvent in serialisableClip.Events)
                {
                    var doesEventReferencesExist = Reference.GetEvent(serialisableClipEvent.ModuleID, serialisableClipEvent.EventID) is not null;

                    if (!doesEventReferencesExist)
                    {
                        IsValid = false;
                        return false;
                    }

                    foreach (var serialisableClipVariable in serialisableClipEvent.Variables)
                    {
                        var doesClipVariableExist = Reference.GetVariable(serialisableClipVariable.ModuleID, serialisableClipVariable.VariableID) is not null;

                        if (!doesClipVariableExist)
                        {
                            IsValid = false;
                            return false;
                        }
                    }
                }
            }
        }

        return false;
    }
}
