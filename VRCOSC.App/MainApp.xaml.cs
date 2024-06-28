using System;
using VRCOSC.App.Utils;

namespace VRCOSC.App;

public partial class MainApp
{
    public MainApp()
    {
        InitializeComponent();

        Logger.AppIdentifier = AppManager.APP_NAME;
        Logger.VersionIdentifier = AppManager.Version;

        AppDomain.CurrentDomain.UnhandledException += (_, e) => ExceptionHandler.Handle((Exception)e.ExceptionObject, "An unhandled exception has occured", true);
    }
}
