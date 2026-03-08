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
                var graphFiles = Directory.EnumerateFiles(graphsPath);
                var localGraphs = new List<NodeGraph>();

                foreach (var graphFilePath in graphFiles)
                {
                    if (Guid.TryParse(Path.GetFileNameWithoutExtension(new FileInfo(graphFilePath).Name), out var graphId))
                    {
                        localGraphs.Add(new NodeGraph { Id = graphId });
                    }
                }

                Graphs.AddRange(localGraphs.OrderBy(g => g.Name.Value));
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
                var presentFiles = Directory.EnumerateFiles(presetsPath);
                var localPresets = new List<NodePreset>();

                foreach (var presetFilePath in presentFiles)
                {
                    if (Guid.TryParse(Path.GetFileNameWithoutExtension(new FileInfo(presetFilePath).Name), out var presetId))
                    {
                        localPresets.Add(new NodePreset { Id = presetId });
                    }
                }

                Presets.AddRange(localPresets.OrderBy(g => g.Name.Value));
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

        Graphs.OnCollectionChanged((newItems, oldItems) => OnGraphsCollectionChanged(newItems, oldItems).Forget());
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

    private async Task OnGraphsCollectionChanged(IEnumerable<NodeGraph> newGraphs, IEnumerable<NodeGraph> oldGraphs)
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

    public async Task TriggerModuleNode(Type nodeType, object[] data)
    {
        if (!Loaded.Value) return;

        foreach (var graph in Graphs)
        {
            await graph.TriggerModuleNode(nodeType, data);
        }
    }
}