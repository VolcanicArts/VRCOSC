// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VRCOSC.App.Nodes;
using VRCOSC.App.UI.Core;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Windows.Nodes;

public partial class VariableCreatorWindow : IManagedWindow
{
    public NodeGraph NodeGraph { get; }

    public Type? VariableType { get; set; }
    public string VariableName { get; set; } = "New Variable";
    public bool VariablePersistent { get; set; }

    public VariableCreatorWindow(NodeGraph nodeGraph)
    {
        NodeGraph = nodeGraph;
        InitializeComponent();
        DataContext = this;

        Title = "Creating Variable";
        updateText(string.Empty);
        GenericArgumentText.Focus();
    }

    public object GetComparer() => new();

    private void GenericArgumentText_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var text = GenericArgumentText.Text;
        updateText(text);
    }

    private void updateText(string text)
    {
        if (!string.IsNullOrWhiteSpace(text) && TypeResolver.TryConstruct(text, out var constructedType) && (!(VariablePersistent && constructedType.IsClass) || constructedType == typeof(string)))
        {
            FormedTypeText.Text = constructedType.GetFriendlyName();
            FormedTypeText.FontStyle = FontStyles.Normal;
            CreateNodeButton.Visibility = Visibility.Visible;
            VariableType = constructedType;
        }
        else
        {
            FormedTypeText.Text = "null";
            FormedTypeText.FontStyle = FontStyles.Italic;
            CreateNodeButton.Visibility = Visibility.Collapsed;
            VariableType = null;
        }
    }

    private void CreateNodeButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void GenericArgumentText_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && VariableType is not null) Close();
    }

    private void ToggleButton_OnChanged(object sender, RoutedEventArgs e)
    {
        var text = GenericArgumentText.Text;
        updateText(text);
    }
}