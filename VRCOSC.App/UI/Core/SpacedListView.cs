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

    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(SpacedListView), new PropertyMetadata(5.0));

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }
}
