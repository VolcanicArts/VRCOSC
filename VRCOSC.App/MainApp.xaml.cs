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
