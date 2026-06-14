// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VRCOSC.App.Nodes;
using VRCOSC.App.UI.Core;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Windows.Nodes;

public partial class NodeCreatorWindow : IManagedWindow
{
    private readonly NodeTypeMetadata metadata;

    public Type? ConstructedType { get; private set; }

    public NodeCreatorWindow(NodeTypeMetadata metadata)
    {
        this.metadata = metadata;
        InitializeComponent();

        Title = $"Creating {metadata.Shared.Name} Node";
        TitleText.Text = metadata.LinkedTypes.Any(t => t.GetGenericArguments().Length > 1) ? "Types:" : "Type:";
        updateText(null);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        GenericArgumentText.Focus();
    }

    private void GenericArgumentText_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        updateText(GenericArgumentText.Text);
    }

    private void updateText(string? text)
    {
        var nonGenericType = metadata.LinkedTypes.SingleOrDefault(t => !t.IsGenericType);

        if (string.IsNullOrEmpty(text) && nonGenericType is not null)
        {
            FormedTypeText.Text = nonGenericType.GetFriendlyName();
            FormedTypeText.FontStyle = FontStyles.Normal;
            ConstructedType = nonGenericType;
            return;
        }

        if (text is not null)
        {
            Type? constructedType = null;

            foreach (var type in metadata.LinkedTypes)
            {
                if (!TypeResolver.TryConstructGenericType(text, type, out constructedType)) continue;

                break;
            }

            if (constructedType is not null)
            {
                FormedTypeText.Text = constructedType.GetFriendlyName();
                FormedTypeText.FontStyle = FontStyles.Normal;
                ConstructedType = constructedType;
                return;
            }
        }

        FormedTypeText.Text = "null";
        FormedTypeText.FontStyle = FontStyles.Italic;
        ConstructedType = null;
    }

    private void GenericArgumentText_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && ConstructedType is not null) Close();
    }

    public object GetComparer() => metadata;
}