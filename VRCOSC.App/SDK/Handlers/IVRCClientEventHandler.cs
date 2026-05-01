// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.VRChat.Logs;

namespace VRCOSC.App.SDK.Handlers;

public interface IVRCClientEventHandler
{
    void HandleClientEvent(IVRChatClientEvent @event);
}