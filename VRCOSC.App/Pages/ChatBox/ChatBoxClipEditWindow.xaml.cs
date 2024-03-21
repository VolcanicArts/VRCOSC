// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using VRCOSC.App.ChatBox.Clips;
using VRCOSC.App.Modules;
using VRCOSC.App.SDK.Modules;

namespace VRCOSC.App.Pages.ChatBox;

public partial class ChatBoxClipEditWindow
{
    public Clip Clip { get; }

    public ChatBoxClipEditWindow(Clip clip)
    {
        InitializeComponent();

        DataContext = clip;
        Clip = clip;

        // TODO: Filter out the ones without states or events
        ModulesList.ItemsSource = ModuleManager.GetInstance().GetModulesOfType<ChatBoxModule>();

        Title = $"Editing {Clip.Name.Value} Clip";
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        for (var index = 0; index < ModulesList.Items.Count; index++)
        {
            var listViewItem = ModulesList.ItemContainerGenerator.ContainerFromIndex(index);
            var module = (Module)ModulesList.Items[index];

            var isLinkedCheckBox = findVisualChild<CheckBox>(listViewItem, "IsLinkedCheckBox")!;
            isLinkedCheckBox.IsChecked = Clip.LinkedModules.Contains(module.SerialisedName);
        }
    }

    private void ModuleSelectionCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var module = (Module)element.Tag;

        if (Clip.LinkedModules.Contains(module.SerialisedName)) return;

        Clip.LinkedModules.Add(module.SerialisedName);
    }

    private void ModuleSelectionCheckBox_UnChecked(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var module = (Module)element.Tag;

        if (!Clip.LinkedModules.Contains(module.SerialisedName)) return;

        Clip.LinkedModules.Remove(module.SerialisedName);
    }

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var textBox = sender as TextBox;
            var selectionStart = textBox.SelectionStart;

            if (textBox.Text.Split(Environment.NewLine).Length < 9)
            {
                textBox.Text = textBox.Text.Insert(selectionStart, Environment.NewLine);
                textBox.SelectionStart = selectionStart + Environment.NewLine.Length;
                textBox.SelectionLength = 0;
            }

            e.Handled = true;
        }
    }

    private T? findVisualChild<T>(DependencyObject parent, string name) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child != null && child is T && ((FrameworkElement)child).Name == name)
                return (T)child;

            var childOfChild = findVisualChild<T>(child, name);
            if (childOfChild != null)
                return childOfChild;
        }

        return null;
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class TextBoxParsingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return str.Replace("\\n", Environment.NewLine);
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return str.Replace(Environment.NewLine, "\\n");
        }

        return value;
    }
}
