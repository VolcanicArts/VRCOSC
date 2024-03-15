using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VRCOSC.App.Modules;
using VRCOSC.App.Packages;
using VRCOSC.App.Profiles;

namespace VRCOSC.App;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

        Title = "VRCOSC 2024.209.0";

        AppManager.GetInstance().Initialise();

        PackageManager.GetInstance().Load().Execute().Wait();
        ProfileManager.GetInstance().Load();
        ModuleManager.GetInstance().LoadAllModules();
    }

    private void MainWindow_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        var focusedElement = FocusManager.GetFocusedElement(this) as FrameworkElement;

        if (e.OriginalSource is not TextBox && focusedElement is TextBox)
        {
            Keyboard.ClearFocus();
        }
    }
}
