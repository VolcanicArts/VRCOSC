// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Controls;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI.Core;

public class SpacedListView : ListView
{
    static SpacedListView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SpacedListView), new FrameworkPropertyMetadata(typeof(SpacedListView)));
    }

    public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(SpacedListView), new PropertyMetadata(0d, SpacingChanged));

    private static void SpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SpacedStackPanel control) control.InvalidateMeasure();
    }

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }
}