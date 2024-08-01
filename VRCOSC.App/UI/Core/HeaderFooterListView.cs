// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

    public static readonly DependencyProperty HideHeaderWhenEmptyProperty =
        DependencyProperty.Register(nameof(HideHeaderWhenEmpty), typeof(bool), typeof(HeaderFooterListView), new PropertyMetadata(false));

    public bool HideHeaderWhenEmpty
    {
        get => (bool)GetValue(HideHeaderWhenEmptyProperty);
        set => SetValue(HideHeaderWhenEmptyProperty, value);
    }

    public static readonly DependencyProperty HideFooterWhenEmptyProperty =
        DependencyProperty.Register(nameof(HideFooterWhenEmpty), typeof(bool), typeof(HeaderFooterListView), new PropertyMetadata(false));

    public bool HideFooterWhenEmpty
    {
        get => (bool)GetValue(HideFooterWhenEmptyProperty);
        set => SetValue(HideFooterWhenEmptyProperty, value);
    }

    public static readonly DependencyProperty ShouldTruncateHeightProperty =
        DependencyProperty.Register(nameof(ShouldTruncateHeight), typeof(bool), typeof(HeaderFooterListView), new PropertyMetadata(true));

    public bool ShouldTruncateHeight
    {
        get => (bool)GetValue(ShouldTruncateHeightProperty);
        set => SetValue(ShouldTruncateHeightProperty, value);
    }
}

public class CollapseWhenListEmptyConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is [bool collapseWhenEmpty, bool isListEmpty])
        {
            if (collapseWhenEmpty && isListEmpty)
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        return Visibility.Visible;
    }

    public object[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => null;
}
