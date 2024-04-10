// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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
            ReferenceClip.States.ForEach(clipState => clipState.UpdateUI());
            ReferenceClip.Events.ForEach(clipEvent => clipEvent.UpdateUI());
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

            var isLinkedCheckBox = listViewItem.FindVisualChild<CheckBox>("IsLinkedCheckBox")!;
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

    private void VariableReferenceDragSource_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var variableReference = (ClipVariableReference)element.Tag;

        DragDrop.DoDragDrop(element, variableReference, DragDropEffects.Copy);
    }

    private void VariableReference_DropTarget(object sender, DragEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var clipElement = (ClipElement)element.Tag;

        if (!e.Data.GetDataPresent(typeof(ClipVariableReference))) return;

        var variableReference = (ClipVariableReference)e.Data.GetData(typeof(ClipVariableReference));
        clipElement.Variables.Add((ClipVariable)Activator.CreateInstance(variableReference.ClipVariableType, variableReference)!);
        clipElement.UpdateUI();
    }

    private const int max_lines = 9;

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var textBox = (sender as TextBox)!;
            var selectionStart = textBox.SelectionLength == 0 ? textBox.CaretIndex : textBox.SelectionStart;

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
        var linesToAdd = Math.Min(remainingLines, newlineCount + 1);
        var lines = pastedText.Split([Environment.NewLine], StringSplitOptions.None);

        var newTextToAdd = string.Join(Environment.NewLine, lines.Take(linesToAdd));
        var newText = textBox.Text.Remove(selectionStart, selectionLength).Insert(selectionStart, newTextToAdd);

        textBox.Text = newText;

        textBox.SelectionStart = selectionStart + newTextToAdd.Length;
        textBox.SelectionLength = 0;

        e.CancelCommand();
    }

    private void VariableSettingButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var variableInstance = (ClipVariable)element.Tag;

        var clipVariableWindow = new ClipVariableEditWindow(variableInstance);
        clipVariableWindow.ShowDialog();
    }

    private void VariableRemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var variableInstance = (ClipVariable)element.Tag;

        var clipState = ReferenceClip.States.FirstOrDefault(clipState => clipState.Variables.Contains(variableInstance));
        clipState?.Variables.Remove(variableInstance);
        clipState?.UpdateUI();

        var clipEvent = ReferenceClip.Events.FirstOrDefault(clipEvent => clipEvent.Variables.Contains(variableInstance));
        clipEvent?.Variables.Remove(variableInstance);
        clipEvent?.UpdateUI();
    }

    private ClipVariable? draggedInstance;

    private void VariableInstance_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var variableInstance = (ClipVariable)element.Tag;

        draggedInstance = variableInstance;
        DragDrop.DoDragDrop(element, new object(), DragDropEffects.Copy);
    }

    private void VariableInstance_DropTarget(object sender, DragEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var droppedInstance = (ClipVariable)element.Tag;

        if (draggedInstance is null) return;

        var draggedClipElement = ReferenceClip.FindElementFromVariable(draggedInstance)!;
        var droppedClipElement = ReferenceClip.FindElementFromVariable(droppedInstance)!;

        if (draggedClipElement != droppedClipElement) return;

        var newIndex = draggedClipElement.Variables.IndexOf(droppedInstance);

        draggedClipElement.Variables.Remove(draggedInstance);
        draggedClipElement.Variables.Insert(newIndex, draggedInstance);

        draggedInstance = null;
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

public class BackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var index = System.Convert.ToInt32(value);
        return index % 2 == 0 ? Application.Current.Resources["CBackground3"] : Application.Current.Resources["CBackground2"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
