﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.VRChat;

namespace VRCOSC.App.SDK.Handlers;

public interface IVRCClientEventHandler
{
    public void OnInstanceEnter(VRChatClientEventInstanceEnter eventArgs);
    public void OnInstanceExit(VRChatClientEventInstanceExit eventArgs);
    public void OnUserJoined(VRChatClientEventUserJoined eventArgs);
    public void OnUserLeft(VRChatClientEventUserLeft eventArgs);
}