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
}
