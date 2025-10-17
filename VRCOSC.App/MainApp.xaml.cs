// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using Windows.Win32;
using VRCOSC.App.Utils;

namespace VRCOSC.App;

public partial class MainApp
{
    public MainApp()
    {
        InitializeComponent();

        AppDomain.CurrentDomain.UnhandledException += (_, e) => ExceptionHandler.Handle((Exception)e.ExceptionObject, "An unhandled exception has occured", true);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var tid = PInvoke.GetCurrentThreadId();
        PInvoke.ImmDisableIME(tid);
        PInvoke.ImmDisableTextFrameService(tid);
    }
}