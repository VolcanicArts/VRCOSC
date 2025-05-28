// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.ObjectModel;
using System.IO;
using VRCOSC.App.SDK.Parameters;

namespace VRCOSC.App.Nodes;

public class NodeManager
{
    private static NodeManager? instance;
    internal static NodeManager GetInstance() => instance ??= new NodeManager();

    private NodeManager()
    {
    }

    public readonly ObservableCollection<NodeField> Fields = [];

    public void Load()
    {
        Fields.Add(new NodeField());
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

    public void OnParameterReceived(VRChatParameter parameter)
    {
        foreach (var nodeField in Fields)
        {
            nodeField.OnParameterReceived(parameter);
        }
    }
}