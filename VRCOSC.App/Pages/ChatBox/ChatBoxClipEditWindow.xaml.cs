// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using System.Windows.Controls;
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
        Console.WriteLine($"Added {module.SerialisedName}");
    }

    private void ModuleSelectionCheckBox_UnChecked(object sender, RoutedEventArgs e)
    {
        var element = (FrameworkElement)sender;
        var module = (Module)element.Tag;

        if (!Clip.LinkedModules.Contains(module.SerialisedName)) return;

        Clip.LinkedModules.Remove(module.SerialisedName);
        Console.WriteLine($"Removed {module.SerialisedName}");
    }

    private T? findVisualChild<T>(DependencyObject parent, string name) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
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
