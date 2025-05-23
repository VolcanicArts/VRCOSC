// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using VRCOSC.App.OSC.VRChat;

namespace VRCOSC.App.Nodes;

public class NodeManager
{
    private static NodeManager? instance;
    internal static NodeManager GetInstance() => instance ??= new NodeManager();

    public ContextMenuRoot FieldContextMenu { get; private set; } = null!;

    private NodeManager()
    {
    }

    public readonly List<NodeField> Fields = [];

    public void Load()
    {
        FieldContextMenu = new ContextMenuRoot();
        FieldContextMenu.Items.Add(ContextMenuBuilder.BuildCreateNodesMenu());

        Fields.Add(new NodeField());
    }

    public void Start()
    {
        foreach (var nodeField in Fields)
        {
            nodeField.Start();
        }
    }

    public void Stop()
    {
        foreach (var nodeField in Fields)
        {
            nodeField.Stop();
        }
    }

    public void ParameterReceived(VRChatOscMessage message)
    {
        foreach (var nodeField in Fields)
        {
            nodeField.ParameterReceived(message);
        }
    }
}