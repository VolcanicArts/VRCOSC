// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.Utils;

namespace VRCOSC.App;

public partial class MainApp
{
    public MainApp()
    {
        InitializeComponent();

        Current.DispatcherUnhandledException += (s, ex) =>
        {
            ExceptionHandler.Handle(ex.Exception, "DispatcherUnhandledException", true);
            ex.Handled = false;
        };

        TaskScheduler.UnobservedTaskException += (s, ex) =>
        {
            ExceptionHandler.Handle(ex.Exception, "UnobservedTaskException", true);
            ex.SetObserved();
        };

        AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
        {
            ExceptionHandler.Handle((ex.ExceptionObject as Exception)!, "An unhandled exception has occured", true);
        };

        AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
        {
            var ex = e.Exception;

            if (ex is PlatformNotSupportedException
                && (ex.StackTrace?.Contains("Windows.UI.ViewManagement.InputPane") ?? false))
                return;

            var txt = ex.ToString();

            if (txt.Contains("ITfThreadMgr") ||
                txt.Contains("TextServicesContext") ||
                txt.Contains("System.Windows.Input.InputMethod"))
            {
                ExceptionHandler.Handle(ex, "FirstChance (input/TSF)");
            }
        };
    }
}