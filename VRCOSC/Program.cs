// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using Velopack;
using VRCOSC.App;

namespace VRCOSC;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        VelopackApp.Build().Run();

        var app = new Application();
        var mainWindow = new MainWindow();
        app.Run(mainWindow);
    }
}
