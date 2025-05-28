// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using VRCOSC.App.Profiles;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public class NodeManager
{
    private static NodeManager? instance;
    internal static NodeManager GetInstance() => instance ??= new NodeManager();

    public ObservableCollection<NodeField> Fields { get; } = [];

    public void Load()
    {
        var loadedFields = Directory.EnumerateFiles(AppManager.GetInstance().Storage.GetFullPath(Path.Join("profiles", ProfileManager.GetInstance().ActiveProfile.Value.ID.ToString(), "nodes")));
        Fields.AddRange(loadedFields.Select(field => new NodeField { Id = Guid.Parse(Path.GetFileNameWithoutExtension(new FileInfo(field).Name)) }));

        if (Fields.Count == 0) Fields.Add(new NodeField());

        foreach (var nodeField in Fields)
        {
            nodeField.Load();
        }
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