using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using VRCOSC_VIRA_Edits__Winui3_;
using VRCOSC.App.Modules;
using VRCOSC.App.UI.Views.Modules;
using VRCOSC.App.UI.Views.Packages;
using VRCOSC.App.UI.Views.Router;
using VRCOSC.App.UI.Views.Settings;
using Windows.Management.Deployment;
using System.Windows;

public class MainWindowAdapter
{
    private readonly MainWindow winUIWindow;

    public MainWindowAdapter(MainWindow winUIWindow)
    {
        this.winUIWindow = winUIWindow;
    }

    public string Title
    {
        get => winUIWindow.m_AppWindow.Title;
        set => winUIWindow.m_AppWindow.Title = value;
    }

    public void Show()
    {
        winUIWindow.Activate();
    }

    public void Close()
    {
        // Add logic if needed to handle the WinUI window closing
        winUIWindow.Close();
    }

    public void SetContent(object userControl)
    {
        // Map WPF user controls to WinUI 3 content pages
        if (userControl is PackagesView)
        {
            // winUIWindow.contentFrame.Navigate(typeof(PackagesPage));
        }
        else if (userControl is ModulesView)
        {
            // winUIWindow.contentFrame.Navigate(typeof(ModulesPage));
        }
        else if (userControl is RouterView)
        {
            // winUIWindow.contentFrame.Navigate(typeof(RouterPage));
        }
        else if (userControl is SettingsView)
        {
            // winUIWindow.contentFrame.Navigate(typeof(SettingsPage));
        }
        // Add additional mappings for other views
    }
}

namespace VRCOSC_VIRA_Edits__Winui3_
{
    public class WPFAppStub : Application
    {
        public WPFAppStub(Window mainWindow)
        {
            MainWindow = mainWindow;
        }
    }
}
