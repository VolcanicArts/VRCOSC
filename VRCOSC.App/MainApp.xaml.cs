// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.Utils;

namespace VRCOSC.App;

public partial class MainApp
{
    public MainApp()
    {
        InitializeComponent();

        AppDomain.CurrentDomain.UnhandledException += (_, e) => ExceptionHandler.Handle((Exception)e.ExceptionObject, "An unhandled exception has occured", true);
    }
}