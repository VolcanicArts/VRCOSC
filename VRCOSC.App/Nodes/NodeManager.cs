// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VRCOSC.App.Profiles;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes;

public class NodeManager
{
    private static NodeManager? instance;
    internal static NodeManager GetInstance() => instance ??= new NodeManager();

    private string graphsPath => AppManager.GetInstance().Storage.GetFullPath(Path.Join("profiles", ProfileManager.GetInstance().ActiveProfile.Value.ID.ToString(), "nodes", "graphs"));
    private string presetsPath => AppManager.GetInstance().Storage.GetFullPath(Path.Join("profiles", ProfileManager.GetInstance().ActiveProfile.Value.ID.ToString(), "nodes", "presets"));

    public ObservableCollection<NodeGraph> Graphs { get; } = [];
    public ObservableCollection<NodePreset> Presets { get; } = [];

    public Action? OnLoad;

    public void Load()
    {
        try
        {
            if (Directory.Exists(graphsPath))
            {
                var loadedGraphs = Directory.EnumerateFiles(graphsPath);
                Graphs.AddRange(loadedGraphs.Select(field => new NodeGraph { Id = Guid.Parse(Path.GetFileNameWithoutExtension(new FileInfo(field).Name)) }).OrderBy(g => g.Name.Value));
            }

            if (Graphs.Count == 0)
            {
                Graphs.Add(new NodeGraph
                {
                    Name = { Value = "Default" }
                });
            }
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, "Unable to load graphs");
        }

        foreach (var graph in Graphs)
        {
            graph.Load();
        }

        try
        {
            if (Directory.Exists(presetsPath))
            {
                var loadedPresets = Directory.EnumerateFiles(presetsPath);
                Presets.AddRange(loadedPresets.Select(presets => new NodePreset { Id = Guid.Parse(Path.GetFileNameWithoutExtension(new FileInfo(presets).Name)) }).OrderBy(p => p.Name.Value));
            }
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, "Unable to load presets");
        }

        foreach (var nodePreset in Presets)
        {
            nodePreset.Load();
        }

        Graphs.OnCollectionChanged(FieldsOnCollectionChanged);
        OnLoad?.Invoke();
    }

    public void ImportGraph(string filePath)
    {
        var graph = new NodeGraph();
        graph.Load(filePath);
        Graphs.Add(graph);
    }

    public void ShowExternally(NodeGraph graph)
    {
        Platform.PresentFile(Path.Join(graphsPath, $"{graph.Id}.json"));
    }

    public void Unload()
    {
        foreach (var nodeGraph in Graphs)
        {
            nodeGraph.Serialise();
        }

        Graphs.Clear();
        Presets.Clear();
    }

    private async void FieldsOnCollectionChanged(IEnumerable<NodeGraph> newGraphs, IEnumerable<NodeGraph> oldGraphs)
    {
        foreach (var newGraph in newGraphs)
        {
            if (AppManager.GetInstance().State.Value == AppManagerState.Started)
            {
                await newGraph.Start();
            }
        }

        foreach (var oldGraph in oldGraphs)
        {
            if (AppManager.GetInstance().State.Value == AppManagerState.Started)
            {
                await oldGraph.Stop();
            }

            try
            {
                File.Delete(Path.Join(graphsPath, $"{oldGraph.Id}.json"));
            }
            catch
            {
            }
        }
    }

    public async Task Start()
    {
        foreach (var graph in Graphs)
        {
            await graph.Start();
        }
    }

    public async Task Stop()
    {
        foreach (var graph in Graphs)
        {
            await graph.Stop();
        }
    }

    public void OnParameterReceived(VRChatParameter parameter)
    {
        foreach (var graph in Graphs)
        {
            graph.OnParameterReceived(parameter);
        }
    }

    public void OnAvatarChange(AvatarConfig? config)
    {
        foreach (var graph in Graphs)
        {
            graph.OnAvatarChange(config);
        }
    }

    public void OnPartialSpeechResult(string result)
    {
        foreach (var graph in Graphs)
        {
            graph.OnPartialSpeechResult(result);
        }
    }

    public void OnFinalSpeechResult(string result)
    {
        foreach (var graph in Graphs)
        {
            graph.OnFinalSpeechResult(result);
        }
    }
}