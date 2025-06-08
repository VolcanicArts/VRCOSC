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

    private string fieldsPath => AppManager.GetInstance().Storage.GetFullPath(Path.Join("profiles", ProfileManager.GetInstance().ActiveProfile.Value.ID.ToString(), "nodes", "fields"));
    private string presetsPath => AppManager.GetInstance().Storage.GetFullPath(Path.Join("profiles", ProfileManager.GetInstance().ActiveProfile.Value.ID.ToString(), "nodes", "presets"));

    public ObservableCollection<NodeField> Fields { get; } = [];
    public ObservableCollection<NodePreset> Presets { get; } = [];

    public void Load()
    {
        if (Directory.Exists(fieldsPath))
        {
            var loadedFields = Directory.EnumerateFiles(fieldsPath);
            Fields.AddRange(loadedFields.Select(field => new NodeField { Id = Guid.Parse(Path.GetFileNameWithoutExtension(new FileInfo(field).Name)) }));
        }

        if (Fields.Count == 0) Fields.Add(new NodeField());

        foreach (var nodeField in Fields)
        {
            nodeField.Load();
        }

        if (Directory.Exists(presetsPath))
        {
            var loadedPresets = Directory.EnumerateFiles(presetsPath);
            Presets.AddRange(loadedPresets.Select(presets => new NodePreset { Id = Guid.Parse(Path.GetFileNameWithoutExtension(new FileInfo(presets).Name)) }));
        }

        foreach (var nodePreset in Presets)
        {
            nodePreset.Load();
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