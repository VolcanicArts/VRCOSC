// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.Modules;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes;

public abstract class Node
{
    internal NodeScape NodeScape { get; set; } = null!;

    public Guid Id { get; } = Guid.NewGuid();
    public ObservableVector2 Position { get; } = new(5000, 5000);
    public int ZIndex { get; set; }

    public NodeMetadata Metadata => NodeScape.GetMetadata(this);
    protected Player Player => AppManager.GetInstance().VRChatClient.Player;

    /// <summary>
    /// Triggers the flow at the specified <paramref name="slot"/>. Optionally scopes any node memory, useful for loops
    /// </summary>
    protected void TriggerFlow(int slot, bool scope = false)
    {
        NodeScape.TriggerOutputFlow(this, slot, scope);
    }
}

public abstract class ModuleNode<T> : Node where T : Module
{
    public T Module => (T)ModuleManager.GetInstance().GetModuleInstanceFromType(typeof(T));
}