// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using VRCOSC.App.ChatBox;
using VRCOSC.App.ChatBox.Clips;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.Modules;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Windows.ChatBox;

public partial class ChatBoxClipEditWindow
{
    public Clip ReferenceClip { get; }

    public bool ShowRelevantModules
    {
        get => SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.ShowRelevantModules);
        set
        {
            SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.ShowRelevantModules).Value = value;
            ReferenceClip.States.ForEach(clipState => clipState.UpdateUI());
            ReferenceClip.Events.ForEach(clipEvent => clipEvent.UpdateUI());
        }
    }

    public ChatBoxClipEditWindow(Clip referenceClip)
    {
        InitializeComponent();

        DataContext = referenceClip;
        ReferenceClip = referenceClip;

        ModulesList.ItemsSource = ModuleManager.GetInstance().Modules.Values.SelectMany(moduleList => moduleList).Where(module => ChatBoxManager.GetInstance().DoesModuleHaveStates(module.FullID)).OrderBy(module => module.IsRemote).ThenBy(module => module.PackageID).ThenBy(module => module.Title);
        ShowRelevantModulesCheckBox.DataContext = this;

        ReferenceClip.Name.Subscribe(newName => Title = $"Editing {newName} Clip", true);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        for (var index = 0; index < ModulesList.Items.Count; index++)
        {
            var listViewItem = ModulesList.ItemContainerGenerator.ContainerFromIndex(index);
            var module = (Module)ModulesList.Items[index];

            var isLinkedCheckBox = listViewItem.FindVisualChild<CheckBox>("IsLinkedCheckBox")!;
            isLinkedCheckBox.IsChecked = ReferenceClip.LinkedModules.Contains(module.FullID);
        }
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        ChatBoxManager.GetInstance().Serialise();
    }

    private void ModuleSelectionCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var module = (Module)element.Tag;

        if (ReferenceClip.LinkedModules.Contains(module.FullID)) return;

        ReferenceClip.LinkedModules.Add(module.FullID);
    }

    private void ModuleSelectionCheckBox_UnChecked(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var module = (Module)element.Tag;

        if (!ReferenceClip.LinkedModules.Contains(module.FullID)) return;

        ReferenceClip.LinkedModules.Remove(module.FullID);
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
        clipElement.Variables.Add(variableReference.CreateInstance());
        clipElement.UpdateUI();
    }

    private void VariableReference_DragEnter(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(typeof(ClipVariableReference)) ? DragDropEffects.Copy : DragDropEffects.None;
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
        var border = (Border)sender;
        var variableInstance = (ClipVariable)border.Tag;

        draggedInstance = variableInstance;
        border.BorderThickness = new Thickness(1);
    }

    private void VariableInstance_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var border = (Border)sender;

        draggedInstance = null;
        border.BorderThickness = new Thickness(0);
    }

    private void VariableInstance_MouseMove(object sender, MouseEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var variableInstance = (ClipVariable)element.Tag;

        if (draggedInstance is not null && e.LeftButton == MouseButtonState.Pressed)
        {
            DragDrop.DoDragDrop(element, variableInstance, DragDropEffects.Move);
        }
    }

    private void VariableInstance_DropTarget(object sender, DragEventArgs e)
    {
        var border = (Border)sender;
        var droppedInstance = (ClipVariable)border.Tag;

        // dragging from reference
        // if (e.Data.GetDataPresent(typeof(ClipVariableReference)))
        // {
        //     var variableReference = (ClipVariableReference)e.Data.GetData(typeof(ClipVariableReference));
        //     var clipElement = ReferenceClip.FindElementFromVariable(droppedInstance)!;
        //     var insertIndex = clipElement.Variables.IndexOf(droppedInstance);
        //     clipElement.Variables.Insert(insertIndex, (ClipVariable)Activator.CreateInstance(variableReference.ClipVariableType, variableReference)!);
        //     clipElement.UpdateUI();
        //     return;
        // }

        // dragging from inside list
        if (draggedInstance is not null)
        {
            var draggedClipElement = ReferenceClip.FindElementFromVariable(draggedInstance)!;
            var droppedClipElement = ReferenceClip.FindElementFromVariable(droppedInstance)!;

            if (draggedClipElement != droppedClipElement) return;

            var newIndex = draggedClipElement.Variables.IndexOf(droppedInstance);

            draggedClipElement.Variables.Move(draggedClipElement.Variables.IndexOf(draggedInstance), newIndex);

            // force whole list to update
            var variables = draggedClipElement.Variables.ToList();
            draggedClipElement.Variables.Clear();
            draggedClipElement.Variables.AddRange(variables);

            draggedInstance = null;
            border.BorderThickness = new Thickness(0);
        }
    }

    private void VariableInstance_DragEnter(object sender, DragEventArgs e)
    {
        var border = (Border)sender;

        if (draggedInstance is not null)
        {
            e.Effects = DragDropEffects.Move;
            border.BorderThickness = new Thickness(1);
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private void VariableInstance_DragLeave(object sender, DragEventArgs e)
    {
        var border = (Border)sender;

        if (draggedInstance is not null)
        {
            e.Effects = DragDropEffects.Move;
            border.BorderThickness = new Thickness(0);
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }
}

public class TextBoxParsingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return str.Replace("\\n", Environment.NewLine);
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return str.Replace(Environment.NewLine, "\\n");
        }

        return value;
    }
}