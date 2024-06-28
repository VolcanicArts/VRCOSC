// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules;

public class AvatarModule : Module
{
    private void avatarChange()
    {
        try
        {
            OnAvatarChange();
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{FullID} has experienced an exception calling {nameof(OnAvatarChange)}");
        }
    }

    internal void PlayerUpdate()
    {
        try
        {
            OnPlayerUpdate();
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{FullID} has experienced an exception calling {nameof(OnPlayerUpdate)}");
        }
    }

    protected virtual void OnAvatarChange()
    {
    }

    protected virtual void OnPlayerUpdate()
    {
    }

    internal override void OnParameterReceived(VRChatOscMessage message)
    {
        if (message.IsAvatarChangeEvent)
        {
            avatarChange();
            return;
        }

        base.OnParameterReceived(message);
    }

    protected internal override void InternalOnRegisteredParameterReceived(RegisteredParameter registeredParameter)
    {
        try
        {
            OnRegisteredParameterReceived(new AvatarParameter(registeredParameter));
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{FullID} has experienced an exception calling {nameof(OnRegisteredParameterReceived)}");
        }
    }

    protected virtual void OnRegisteredParameterReceived(AvatarParameter avatarParameter)
    {
    }
}
