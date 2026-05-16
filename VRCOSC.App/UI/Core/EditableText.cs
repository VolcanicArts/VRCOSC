// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace VRCOSC.App.UI.Core;

public class EditableText : Control
{
    private TextBox? editTextBox;
    private Grid? displayGrid;

    static EditableText()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(EditableText),
            new FrameworkPropertyMetadata(typeof(EditableText)));
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(EditableText),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty IsEditingProperty =
        DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(EditableText),
            new PropertyMetadata(false, OnIsEditingChanged));

    public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(EditableText),
            new PropertyMetadata(20.0));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsEditing
    {
        get => (bool)GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    public new double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly RoutedEvent TextChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(TextChanged), RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<string>), typeof(EditableText));

    public event RoutedPropertyChangedEventHandler<string> TextChanged
    {
        add => AddHandler(TextChangedEvent, value);
        remove => RemoveHandler(TextChangedEvent, value);
    }

    public static readonly RoutedEvent EditCompletedEvent =
        EventManager.RegisterRoutedEvent(nameof(EditCompleted), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(EditableText));

    public event RoutedEventHandler EditCompleted
    {
        add => AddHandler(EditCompletedEvent, value);
        remove => RemoveHandler(EditCompletedEvent, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (displayGrid != null)
            displayGrid.MouseDown -= DisplayGrid_MouseDown;

        if (editTextBox != null)
        {
            editTextBox.LostFocus -= EditTextBox_LostFocus;
            editTextBox.KeyDown -= EditTextBox_KeyDown;
        }

        displayGrid = GetTemplateChild("PART_DisplayGrid") as Grid;
        editTextBox = GetTemplateChild("PART_EditTextBox") as TextBox;

        if (displayGrid != null)
            displayGrid.MouseDown += DisplayGrid_MouseDown;

        if (editTextBox != null)
        {
            editTextBox.LostFocus += EditTextBox_LostFocus;
            editTextBox.KeyDown += EditTextBox_KeyDown;
        }
    }

    private static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EditableText control && (bool)e.NewValue)
        {
            control.Dispatcher.BeginInvoke(new Action(() =>
            {
                control.editTextBox?.Focus();
            }), DispatcherPriority.Input);
        }
    }

    private void DisplayGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            e.Handled = true;
            IsEditing = true;
        }
    }

    private void EditTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        completeEdit();
    }

    private void EditTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            completeEdit();
            Focus();
        }

        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            IsEditing = false;
        }
    }

    private void completeEdit()
    {
        var oldValue = Text;

        IsEditing = false;
        Text = editTextBox?.Text ?? string.Empty;

        if (oldValue != Text)
            RaiseEvent(new RoutedPropertyChangedEventArgs<string>(oldValue, Text, TextChangedEvent));

        RaiseEvent(new RoutedEventArgs(EditCompletedEvent));
    }
}