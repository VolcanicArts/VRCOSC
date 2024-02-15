using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VRCOSC.Packages;

namespace VRCOSC;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

        Title = "VRCOSC 2024.209.0";

        PackageManager.GetInstance().Load().Execute().Wait();
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
