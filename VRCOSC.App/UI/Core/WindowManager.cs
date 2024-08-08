using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Core;

public class WindowManager
{
    private readonly List<Window> childWindows = new();
    private bool removeFromList = true;

    public WindowManager(Window parentWindow)
    {
        parentWindow.Closing += parentWindowOnClosing;
    }

    public WindowManager(DependencyObject dp)
    {
        var parentWindow = Window.GetWindow(dp);

        if (parentWindow is null)
        {
            ExceptionHandler.Handle("Please construct the WindowManager in the Loaded event and not the constructor");
            return;
        }

        parentWindow.Closing += parentWindowOnClosing;
    }

    private void parentWindowOnClosing(object? sender, CancelEventArgs e)
    {
        removeFromList = false;

        foreach (var childWindow in childWindows)
        {
            childWindow.Close();
        }
    }

    private void childWindowOnClosed(object? sender, EventArgs e)
    {
        if (!removeFromList) return;

        childWindows.Remove((Window)sender!);
    }

    public void SpawnChild(Window childWindow)
    {
        childWindow.Closed += childWindowOnClosed;
        childWindows.Add(childWindow);

        childWindow.Show();
    }

    public bool TrySpawnChild(Window childWindow)
    {
        var existingWindow = childWindows.FirstOrDefault(compareWindow => compareWindow.GetType() == childWindow.GetType());

        if (existingWindow is not null)
        {
            existingWindow.WindowState = WindowState.Normal;
            existingWindow.Focus();
            return false;
        }

        SpawnChild(childWindow);
        return true;
    }
}
