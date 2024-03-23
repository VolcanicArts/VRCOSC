// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using VRCOSC.App.ChatBox.Clips;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.Modules;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Pages.ChatBox;

public partial class ChatBoxClipEditWindow
{
    public Clip ReferenceClip { get; }

    public bool ShowRelevantModules
    {
        get => SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.ShowRelevantModules);
        set
        {
            SettingsManager.GetInstance().GetObservable(VRCOSCSetting.ShowRelevantModules).Value = value;
            ReferenceClip.States.ForEach(clipState => clipState.UpdateVisibility());
            ReferenceClip.Events.ForEach(clipEvent => clipEvent.UpdateVisibility());
        }
    }

    public ChatBoxClipEditWindow(Clip referenceClip)
    {
        InitializeComponent();

        DataContext = referenceClip;
        ReferenceClip = referenceClip;

        // TODO: Filter out the ones without states or events
        ModulesList.ItemsSource = ModuleManager.GetInstance().GetModulesOfType<ChatBoxModule>();
        ShowRelevantModulesCheckBox.DataContext = this;

        Title = $"Editing {ReferenceClip.Name.Value} Clip";
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        for (var index = 0; index < ModulesList.Items.Count; index++)
        {
            var listViewItem = ModulesList.ItemContainerGenerator.ContainerFromIndex(index);
            var module = (Module)ModulesList.Items[index];

            var isLinkedCheckBox = findVisualChild<CheckBox>(listViewItem, "IsLinkedCheckBox")!;
            isLinkedCheckBox.IsChecked = ReferenceClip.LinkedModules.Contains(module.SerialisedName);
        }
    }

    private void ModuleSelectionCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var module = (Module)element.Tag;

        if (ReferenceClip.LinkedModules.Contains(module.SerialisedName)) return;

        ReferenceClip.LinkedModules.Add(module.SerialisedName);
    }

    private void ModuleSelectionCheckBox_UnChecked(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var module = (Module)element.Tag;

        if (!ReferenceClip.LinkedModules.Contains(module.SerialisedName)) return;

        ReferenceClip.LinkedModules.Remove(module.SerialisedName);
    }

    private void dragSource_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var variableReference = (ClipVariableReference)element.Tag;

        DragDrop.DoDragDrop(element, variableReference, DragDropEffects.Copy);
    }

    private void dropTarget_Drop(object sender, DragEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var clipElement = (ClipElement)element.Tag;

        var variableReference = (ClipVariableReference)e.Data.GetData(typeof(ClipVariableReference));
        clipElement.Variables.Add((ClipVariable)Activator.CreateInstance(variableReference.ClipVariableType, variableReference)!);
    }

    private const int max_lines = 9;

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var textBox = (sender as TextBox)!;
            var selectionStart = textBox.SelectionStart;

            if (textBox.Text.Split(Environment.NewLine).Length < max_lines)
            {
                textBox.Text = textBox.Text.Insert(selectionStart, Environment.NewLine);
                textBox.SelectionStart = selectionStart + Environment.NewLine.Length;
                textBox.SelectionLength = 0;
            }

            e.Handled = true;
        }
    }

    private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(DataFormats.UnicodeText)) return;

        var textBox = (sender as TextBox)!;
        var selectionStart = textBox.SelectionStart;
        var selectionLength = textBox.SelectionLength;

        var pastedText = e.DataObject.GetData(DataFormats.UnicodeText) as string ?? string.Empty;

        var newlineCount = pastedText.Split([Environment.NewLine], StringSplitOptions.None).Length - 1;
        var currentLineCount = textBox.LineCount;

        var selectedText = textBox.Text.Substring(selectionStart, selectionLength);
        var selectedLineCount = selectedText.Split([Environment.NewLine], StringSplitOptions.None).Length;

        var remainingLines = Math.Max(max_lines - (currentLineCount - selectedLineCount), 0);
        var linesToAdd = Math.Min(remainingLines, newlineCount + 1); // Add one to account for the first line
        var lines = pastedText.Split([Environment.NewLine], StringSplitOptions.None);

        var newTextToAdd = string.Join(Environment.NewLine, lines.Take(linesToAdd));
        var newText = textBox.Text.Remove(selectionStart, selectionLength).Insert(selectionStart, newTextToAdd);

        textBox.Text = newText;

        textBox.SelectionStart = selectionStart + newTextToAdd.Length;
        textBox.SelectionLength = 0;

        e.CancelCommand();
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
