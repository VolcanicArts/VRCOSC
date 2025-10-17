// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VRCOSC.App.Modules;
using VRCOSC.App.Profiles;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.UI.Windows;
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

    public Action? OnLoading;
    public Observable<bool> Loaded { get; } = new();

    public void Load()
    {
        OnLoading?.Invoke();

        if (ModuleManager.GetInstance().ErrorsInLastLoad.Value)
        {
            Loaded.Value = false;
            Graphs.Clear();
            Presets.Clear();
            return;
        }

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

        Graphs.OnCollectionChanged(OnGraphsCollectionChanged);
        Presets.OnCollectionChanged(OnPresetsCollectionChanged);
        Loaded.Value = true;
    }

    public void ImportGraph(string filePath)
    {
        var graph = new NodeGraph();
        graph.Load(filePath);
        Graphs.Add(graph);
    }

    public void ImportPreset(string filePath)
    {
        var preset = new NodePreset();
        preset.Load(filePath);
        Presets.Add(preset);
    }

    public void ShowExternally(NodeGraph graph)
    {
        Platform.PresentFile(Path.Join(graphsPath, $"{graph.Id}.json"));
    }

    public void ShowExternally(NodePreset preset)
    {
        Platform.PresentFile(Path.Join(presetsPath, $"{preset.Id}.json"));
    }

    public void Unload()
    {
        if (!Loaded.Value) return;

        foreach (var nodeGraph in Graphs)
        {
            nodeGraph.Serialise();
        }

        Graphs.Clear();
        Presets.Clear();
    }

    private async void OnGraphsCollectionChanged(IEnumerable<NodeGraph> newGraphs, IEnumerable<NodeGraph> oldGraphs)
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

    private void OnPresetsCollectionChanged(IEnumerable<NodePreset> newPresets, IEnumerable<NodePreset> oldPresets)
    {
        foreach (var oldPreset in oldPresets)
        {
            try
            {
                File.Delete(Path.Join(presetsPath, $"{oldPreset.Id}.json"));
            }
            catch
            {
            }
        }

        MainWindow.GetInstance().NodesView.RefreshAllContextMenus();
    }

    public async Task Start()
    {
        if (!Loaded.Value) return;

        foreach (var graph in Graphs)
        {
            await graph.Start();
        }
    }

    public async Task Stop()
    {
        if (!Loaded.Value) return;

        foreach (var graph in Graphs)
        {
            await graph.Stop();
        }
    }

    public void OnParameterReceived(VRChatParameter parameter)
    {
        if (!Loaded.Value) return;

        foreach (var graph in Graphs)
        {
            graph.OnParameterReceived(parameter);
        }
    }

    public void OnAvatarChange(AvatarConfig? config)
    {
        if (!Loaded.Value) return;

        foreach (var graph in Graphs)
        {
            graph.OnAvatarChange(config);
        }
    }

    public void OnPartialSpeechResult(string result)
    {
        if (!Loaded.Value) return;

        foreach (var graph in Graphs)
        {
            graph.OnPartialSpeechResult(result);
        }
    }

    public void OnFinalSpeechResult(string result)
    {
        if (!Loaded.Value) return;

        foreach (var graph in Graphs)
        {
            graph.OnFinalSpeechResult(result);
        }
    }
}