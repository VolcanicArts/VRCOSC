// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using Velopack;

namespace VRCOSC.Desktop;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        VelopackApp.Build().Run();

        var app = new App();
        var mainWindow = new MainWindow();
        app.Run(mainWindow);
    }
}
