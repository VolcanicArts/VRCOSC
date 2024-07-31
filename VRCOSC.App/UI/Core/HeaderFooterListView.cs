using System.Windows;
using System.Windows.Controls;

// ReSharper disable InconsistentNaming

namespace VRCOSC.App.UI.Core;

public class HeaderFooterListView : ListView
{
    static HeaderFooterListView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HeaderFooterListView), new FrameworkPropertyMetadata(typeof(HeaderFooterListView)));
    }

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(HeaderFooterListView), new PropertyMetadata(null));

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(nameof(Footer), typeof(object), typeof(HeaderFooterListView), new PropertyMetadata(null));

    public object Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }
}
