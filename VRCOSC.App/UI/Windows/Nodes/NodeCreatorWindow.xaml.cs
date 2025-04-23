// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.UI.Core;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Windows.Nodes;

public partial class NodeCreatorWindow : IManagedWindow
{
    public NodeScape NodeScape { get; }
    public Type NodeType { get; }
    //public Dictionary<string, Type> CommonTypesSource => NodeConstants.COMMON_TYPES.ToDictionary(type => type.GetFriendlyName(), type => type);
    public Dictionary<string, Type> CommonTypesSource => new();

    public Type? ConstructedType;

    public NodeCreatorWindow(NodeScape nodeScape, Type nodeType)
    {
        NodeScape = nodeScape;
        NodeType = nodeType;
        InitializeComponent();
        DataContext = this;

        Title = $"Creating {nodeType.GetFriendlyName()}";
        updateText(string.Empty);
    }

    public object GetComparer() => NodeType;

    private void GenericArgumentText_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var text = GenericArgumentText.Text;
        updateText(text);
    }

    private void updateText(string text)
    {
        if (TypeResolver.TryConstructGenericType(text, NodeType, out var constructedType))
        {
            FormedTypeText.Text = constructedType.GetFriendlyName();
            FormedTypeText.FontStyle = FontStyles.Normal;
            CreateNodeButton.Visibility = Visibility.Visible;
            ConstructedType = constructedType;
        }
        else
        {
            FormedTypeText.Text = "null";
            FormedTypeText.FontStyle = FontStyles.Italic;
            CreateNodeButton.Visibility = Visibility.Collapsed;
            ConstructedType = null;
        }
    }

    private void CreateNodeButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}