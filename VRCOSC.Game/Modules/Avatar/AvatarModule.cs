// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.Game.Modules.Attributes;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.Modules.Avatar;

public abstract class AvatarModule : Module
{
    protected void CreateParameter<T>(Enum lookup, ParameterMode mode, string parameterName, string displayName, string description)
        => Parameters.Add(lookup, new AvatarModuleParameter
        {
            Name = displayName,
            Description = description,
            Default = parameterName,
            DependsOn = null,
            Mode = mode,
            ExpectedType = typeof(T)
        });

    internal void ChatBoxUpdate()
    {
        ChatBoxUpdateMethods.ForEach(UpdateMethod);
    }

    internal void PlayerUpdate()
    {
        try
        {
            OnPlayerUpdate();
        }
        catch (Exception e)
        {
            PushException(e);
        }
    }

    protected virtual void OnAvatarChange() { }
    protected virtual void OnPlayerUpdate() { }

    internal override void OnParameterReceived(VRChatOscMessage message)
    {
        if (message.IsAvatarChangeEvent)
        {
            avatarChange();
            return;
        }

        base.OnParameterReceived(message);
    }

    private void avatarChange()
    {
        try
        {
            OnAvatarChange();
        }
        catch (Exception e)
        {
            PushException(e);
        }
    }

    protected internal override void ModuleParameterReceived(RegisteredParameter parameter) => OnRegisteredParameterReceived(new AvatarParameter(parameter));

    protected virtual void OnRegisteredParameterReceived(AvatarParameter avatarParameter) { }
}
