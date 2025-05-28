// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VRCOSC.App.Nodes;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Nodes;

public partial class NodesView
{
    public ObservableCollection<NodeFieldViewModel> NodeFieldsSource { get; } = [];

    private bool sidePanelOpen;

    public NodesView()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += OnLoaded;
        NodeManager.GetInstance().Fields.OnCollectionChanged(OnFieldsCollectionChanged, true);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        showNodeField(0);
    }

    private void OnFieldsCollectionChanged(IEnumerable<NodeField> newFields, IEnumerable<NodeField> oldFields)
    {
        foreach (var nodeField in oldFields)
        {
            NodeFieldsSource.RemoveIf(vm => vm.NodeField == nodeField);
        }

        NodeFieldsSource.AddRange(newFields.Select(field => new NodeFieldViewModel(field)));
    }

    private void FieldButton_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var index = (int)element.Tag;

        showNodeField(index);
    }

    private void showNodeField(int index)
    {
        for (var i = 0; i < NodeFieldsSource.Count; i++)
        {
            var nodeFieldViewModel = NodeFieldsSource[i];
            nodeFieldViewModel.Selected = i == index;
        }
    }

    private void SidePanelButton_OnClick(object sender, RoutedEventArgs e)
    {
        var from = PanelTransform.X;
        var to = sidePanelOpen ? -SidePanel.ActualWidth : 0;

        var anim = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
        };

        PanelTransform.BeginAnimation(TranslateTransform.XProperty, anim);

        sidePanelOpen = !sidePanelOpen;
    }
}

public class NodeFieldViewModel : INotifyPropertyChanged
{
    public NodeField NodeField { get; }

    private bool selected;

    public bool Selected
    {
        get => selected;
        set
        {
            selected = value;
            OnPropertyChanged();
        }
    }

    public NodeFieldViewModel(NodeField nodeField)
    {
        NodeField = nodeField;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}