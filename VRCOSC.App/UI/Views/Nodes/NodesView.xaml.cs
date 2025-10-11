// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VRCOSC.App.Nodes;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Nodes;

public partial class NodesView
{
    public ObservableCollection<NodeGraph> GraphsSource => NodeManager.GetInstance().Graphs;
    public ObservableCollection<NodePreset> PresetsSource => NodeManager.GetInstance().Presets;
    private Dictionary<Guid, NodeGraphView> viewCache { get; } = [];

    public Observable<bool> NodesLoaded => NodeManager.GetInstance().Loaded;

    private NodeGraph? selectedGraph;

    public NodesView()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += OnLoaded;

        NodeManager.GetInstance().OnLoading += () =>
        {
            viewCache.Clear();
            selectedGraph = null;
        };
    }

    private void setActiveTab(bool presetTab)
    {
        if (presetTab)
        {
            PresetsTab.Background = (Brush)FindResource("CBackground3");
            GraphsTab.Background = (Brush)FindResource("CBackground2");
            PresetsPanel.Visibility = Visibility.Visible;
            GraphsPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            GraphsTab.Background = (Brush)FindResource("CBackground3");
            PresetsTab.Background = (Brush)FindResource("CBackground2");
            GraphsPanel.Visibility = Visibility.Visible;
            PresetsPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!NodesLoaded.Value) return;

        setActiveTab(false);
        showNodeGraph(selectedGraph ?? GraphsSource.First());
    }

    private async void showNodeGraph(NodeGraph nodeGraph)
    {
        if (!viewCache.TryGetValue(nodeGraph.Id, out var view))
        {
            view = new NodeGraphView(nodeGraph);
            viewCache[nodeGraph.Id] = view;
        }

        ActiveField.Content = view;
        await Dispatcher.Yield(DispatcherPriority.Loaded);

        if (selectedGraph is not null)
            selectedGraph.Selected.Value = false;

        nodeGraph.Selected.Value = true;
        selectedGraph = nodeGraph;
    }

    private void CreateGraph_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        var newGraph = new NodeGraph();
        NodeManager.GetInstance().Graphs.Add(newGraph);
        showNodeGraph(newGraph);
    }

    private async void ImportGraph_OnClick(object sender, RoutedEventArgs e)
    {
        var filePath = await Platform.PickFileAsync(".json");
        if (filePath is null) return;

        NodeManager.GetInstance().ImportGraph(filePath);
    }

    private void GraphTab_OnClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        var element = (FrameworkElement)sender;
        var graph = (NodeGraph)element.Tag;

        if (e.ChangedButton == MouseButton.Left)
            showNodeGraph(graph);
    }

    private void DeleteGraph_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        var element = (FrameworkElement)sender;
        var graph = (NodeGraph)element.Tag;

        if (NodeManager.GetInstance().Graphs.Count == 1) return;

        var result = MessageBox.Show("Are you sure you want to delete this graph?", "Graph Delete Warning", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;

        var index = Math.Max(0, NodeManager.GetInstance().Graphs.IndexOf(graph) - 1);

        NodeManager.GetInstance().Graphs.Remove(graph);

        if (selectedGraph == graph)
        {
            showNodeGraph(NodeManager.GetInstance().Graphs[index]);
        }
    }

    private void GraphsTab_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        if (e.ChangedButton == MouseButton.Left)
            setActiveTab(false);
    }

    private void PresetsTab_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        if (e.ChangedButton == MouseButton.Left)
            setActiveTab(true);
    }

    private void ExportGraph_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        var element = (FrameworkElement)sender;
        var graph = (NodeGraph)element.Tag;

        NodeManager.GetInstance().ShowExternally(graph);
    }

    private async void ImportPreset_OnClick(object sender, RoutedEventArgs e)
    {
        var filePath = await Platform.PickFileAsync(".json");
        if (filePath is null) return;

        NodeManager.GetInstance().ImportPreset(filePath);

        // we need to refresh the context menu to add the imported preset
        foreach (var nodeGraph in GraphsSource.Where(g => g.UILoaded))
        {
            nodeGraph.MarkDirty();
        }
    }

    private void ExportPreset_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        var element = (FrameworkElement)sender;
        var preset = (NodePreset)element.Tag;

        NodeManager.GetInstance().ShowExternally(preset);
    }

    private void DeletePreset_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        var element = (FrameworkElement)sender;
        var preset = (NodePreset)element.Tag;

        var result = MessageBox.Show("Are you sure you want to delete this preset?", "Preset Delete Warning", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;

        NodeManager.GetInstance().Presets.Remove(preset);

        // we need to refresh the context menu to remove the deleted preset
        foreach (var nodeGraph in GraphsSource.Where(g => g.UILoaded))
        {
            nodeGraph.MarkDirty();
        }
    }
}