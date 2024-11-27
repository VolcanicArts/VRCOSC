// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace VRCOSC.App.UI.Core;

public class WindowManager
{
    private readonly List<IManagedWindow> childWindows = [];

    public WindowManager(Window parentWindow)
    {
        parentWindow.Closing += parentWindowClosing;
    }

    public WindowManager(DependencyObject dp)
    {
        var parentWindow = Window.GetWindow(dp);
        if (parentWindow is null) throw new InvalidOperationException("Please construct the WindowManager in the Loaded event and not the constructor");

        parentWindow.Closing += parentWindowClosing;
    }

    private void parentWindowClosing(object? sender, CancelEventArgs e)
    {
        foreach (var childWindow in childWindows.ToList().Cast<Window>())
        {
            childWindow.Close();
        }
    }

    private void childWindowClosed(object? sender, EventArgs e)
    {
        childWindows.Remove((IManagedWindow)sender!);
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
        window.Show();

        childWindows.Add(childWindow);
    }
}

public interface IManagedWindow
{
    public object GetComparer();
}