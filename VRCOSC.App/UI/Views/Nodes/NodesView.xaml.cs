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
    public ObservableCollection<NodeField> NodeFieldsSource => NodeManager.GetInstance().Fields;
    private Dictionary<Guid, NodeFieldView> viewCache { get; } = [];

    private NodeField? selectedNodeField;

    public NodesView()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        showNodeField(NodeFieldsSource.First());
    }

    private async void showNodeField(NodeField nodeField)
    {
        if (!viewCache.TryGetValue(nodeField.Id, out var view))
        {
            view = new NodeFieldView(nodeField);
            viewCache[nodeField.Id] = view;
        }

        ActiveField.Content = view;
        await Dispatcher.Yield(DispatcherPriority.Loaded);
        view.FocusGrid();

        selectedNodeField = nodeField;
    }

    private void CreateField_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        var newField = new NodeField();
        NodeManager.GetInstance().Fields.Add(newField);
        showNodeField(newField);
    }

    private void FieldTab_OnClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;

        var element = (FrameworkElement)sender;
        var nodeField = (NodeField)element.Tag;

        showNodeField(nodeField);
    }

    private void DeleteField_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        var element = (FrameworkElement)sender;
        var nodeField = (NodeField)element.Tag;

        if (NodeManager.GetInstance().Fields.Count == 1) return;

        var index = Math.Max(0, NodeManager.GetInstance().Fields.IndexOf(nodeField) - 1);

        NodeManager.GetInstance().Fields.Remove(nodeField);

        if (selectedNodeField == nodeField)
        {
            showNodeField(NodeManager.GetInstance().Fields[index]);
        }
    }
}