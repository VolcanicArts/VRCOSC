// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using Velopack;
using VRCOSC.App;
using VRCOSC.App.UI.Windows;

namespace VRCOSC;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();

        var app = new MainApp();
        var mainWindow = new MainWindow(args);
        app.Run(mainWindow);
    }
}