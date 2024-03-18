using System;
using VRCOSC.App.Utils;

namespace VRCOSC.App;

public partial class MainApp
{
    public MainApp()
    {
        InitializeComponent();

        AppDomain.CurrentDomain.UnhandledException += (_, e) => Logger.Error((Exception)e.ExceptionObject, "An unhandled error has occured");
    }
}
