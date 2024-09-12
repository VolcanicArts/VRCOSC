// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Controls;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI.Core;

public class TitleDescriptionContent : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(object), typeof(TitleDescriptionContent), new PropertyMetadata(string.Empty));

    public object Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(object), typeof(TitleDescriptionContent), new PropertyMetadata(string.Empty));

    public object Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    static TitleDescriptionContent()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TitleDescriptionContent), new FrameworkPropertyMetadata(typeof(TitleDescriptionContent)));
    }
}
