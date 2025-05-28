// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using VRCOSC.App.Nodes;

namespace VRCOSC.App.UI.Views.Nodes;

public partial class NodesView
{
    public ObservableCollection<NodeField> NodeFieldsSource => NodeManager.GetInstance().Fields;
    private Dictionary<Guid, NodeFieldView> viewCache { get; } = [];

    private bool sidePanelOpen;

    public NodesView()
    {
        InitializeComponent();
        DataContext = this;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        showNodeField(0);
    }

    private void FieldButton_OnClick(object sender, RoutedEventArgs routedEventArgs)
    {
        var element = (FrameworkElement)sender;
        var index = (int)element.Tag;

        showNodeField(index);
    }

    private async void showNodeField(int index)
    {
        var nodeField = NodeFieldsSource[index];

        if (!viewCache.TryGetValue(nodeField.Id, out var view))
        {
            view = new NodeFieldView(nodeField);
            viewCache[nodeField.Id] = view;
        }

        ActiveField.Content = view;
        await Dispatcher.Yield(DispatcherPriority.Loaded);
        view.FocusMainContainer();
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