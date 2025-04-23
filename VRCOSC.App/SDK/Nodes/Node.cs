// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes;

public abstract class Node
{
    public NodeScape NodeScape { get; set; } = null!;

    public Guid Id { get; } = Guid.NewGuid();
    public ObservableVector2 Position { get; } = new(5000, 5000);
    public int ZIndex { get; set; }

    public NodeMetadata Metadata => NodeScape.GetMetadata(this);
    protected Player Player => AppManager.GetInstance().VRChatClient.Player;
}