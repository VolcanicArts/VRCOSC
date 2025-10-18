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

        AppDomain.CurrentDomain.FirstChanceException += (s, ex) =>
        {
            if (ex.Exception.Source?.Contains("PresentationCore") == true ||
                ex.Exception.ToString().Contains("InputMethod") ||
                ex.Exception.ToString().Contains("ITfThreadMgr"))
            {
                ExceptionHandler.Handle(ex.Exception, "FirstChanceException (input/TSF)");
            }
        };
    }
}