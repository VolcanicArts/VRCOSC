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

    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(HeaderFooterListView), new PropertyMetadata(null));

    public DataTemplate HeaderTemplate
    {
        get => (DataTemplate)GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    public static readonly DependencyProperty FooterTemplateProperty =
        DependencyProperty.Register(nameof(FooterTemplate), typeof(DataTemplate), typeof(HeaderFooterListView), new PropertyMetadata(null));

    public DataTemplate FooterTemplate
    {
        get => (DataTemplate)GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }
}
