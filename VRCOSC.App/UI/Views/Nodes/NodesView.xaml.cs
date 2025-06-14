// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using VRCOSC.App.Nodes;

namespace VRCOSC.App.UI.Views.Nodes;

public partial class NodesView
{
    public ObservableCollection<NodeGraph> NodeGraphsSource => NodeManager.GetInstance().Graphs;
    private Dictionary<Guid, NodeGraphView> viewCache { get; } = [];

    private NodeGraph? selectedGraph;

    public NodesView()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        showNodeGraph(NodeGraphsSource.First());
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
        view.FocusGrid();

        selectedGraph = nodeGraph;
    }

    private void CreateGraph_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        var newGraph = new NodeGraph();
        NodeManager.GetInstance().Graphs.Add(newGraph);
        showNodeGraph(newGraph);
    }

    private void GraphTab_OnClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        var element = (FrameworkElement)sender;
        var graph = (NodeGraph)element.Tag;

        showNodeGraph(graph);
    }

    private void DeleteGraph_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        var element = (FrameworkElement)sender;
        var graph = (NodeGraph)element.Tag;

        if (NodeManager.GetInstance().Graphs.Count == 1) return;

        var index = Math.Max(0, NodeManager.GetInstance().Graphs.IndexOf(graph) - 1);

        NodeManager.GetInstance().Graphs.Remove(graph);

        if (selectedGraph == graph)
        {
            showNodeGraph(NodeManager.GetInstance().Graphs[index]);
        }
    }
}