// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Core;

public class WindowManager
{
    private readonly Window parentWindow;
    private readonly List<IManagedWindow> childWindows = [];

    public WindowManager(Window parentWindow)
    {
        this.parentWindow = parentWindow;
        parentWindow.Closing += parentWindowClosing;
    }

    public WindowManager(DependencyObject dp)
    {
        parentWindow = Window.GetWindow(dp) ?? throw new InvalidOperationException("Please construct the WindowManager in the Loaded event and not the constructor");
        parentWindow.Closing += parentWindowClosing;
    }

    private void parentWindowClosing(object? sender, CancelEventArgs e)
    {
        if (e.Cancel) return;

        foreach (var childWindow in childWindows.ToList().Cast<Window>())
        {
            childWindow.Close();
        }
    }

    private void childWindowClosed(object? sender, EventArgs e)
    {
        childWindows.Remove((IManagedWindow)sender!);
    }

    private void childWindowOnSourceInitialized(object? sender, EventArgs e)
    {
        var window = (Window)sender!;
        window.ApplyDefaultStyling();
        window.SetPositionFrom(parentWindow);
    }

    public void TrySpawnChild(IManagedWindow childWindow)
    {
        if (childWindow is not Window) throw new InvalidOperationException($"An {nameof(IManagedWindow)} must extend {nameof(Window)}");

        var window = (Window?)childWindows.FirstOrDefault(compareWindow => compareWindow.GetComparer() == childWindow.GetComparer());

        if (window is not null)
        {
            window.WindowState = WindowState.Normal;
            window.Focus();
            return;
        }

        window = (Window)childWindow;
        window.Closed += childWindowClosed;
        window.SourceInitialized += childWindowOnSourceInitialized;
        window.Show();

        childWindows.Add(childWindow);
    }
}

public interface IManagedWindow
{
    public object GetComparer();
}