// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.Modules;
using VRCOSC.App.Nodes;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Nodes;

public abstract class Node
{
    internal NodeField NodeField { get; set; } = null!;
    internal NodeVariableSize VariableSize => NodeField.VariableSizes[Id];

    public Guid Id { get; } = Guid.NewGuid();
    public ObservableVector2 Position { get; } = new(5000, 5000);
    public int ZIndex { get; set; }

    public NodeMetadata Metadata => NodeField.GetMetadata(this);
    protected Player Player => AppManager.GetInstance().VRChatClient.Player;

    protected void TriggerSelf()
    {
        NodeField.StartFlow(this);
    }

    protected Task TriggerFlow(CancellationToken token, int slot, bool scope = false)
    {
        return NodeField.TriggerOutputFlow(this, token, slot, scope);
    }
}

public abstract class ModuleNode<T> : Node where T : Module
{
    public T Module => (T)ModuleManager.GetInstance().GetModuleInstanceFromType(typeof(T));
}