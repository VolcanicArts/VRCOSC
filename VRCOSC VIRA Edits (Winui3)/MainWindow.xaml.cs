using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI;
using Microsoft.UI.Windowing; // Needed for AppWindow.
using WinRT.Interop;          // Needed for XAML/HWND interop.
using VRCOSC_VIRA_Edits__Winui3_.Views;
using VRCOSC.App;
using VRCOSC.App.Settings;
using System.Reflection;
using VRCOSC.App.Audio.Whisper;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.OSC;
using VRCOSC.App.SDK.OVR;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Packages;
using VRCOSC.App.Updater;
using VRCOSC.App.Utils;
using VRCOSC.App.VRChatAPI;
using VRCOSC.App.Modules;
using VRCOSC.App.UI.Views.AppDebug;
using VRCOSC.App.UI.Views.AppSettings;
using VRCOSC.App.UI.Views.ChatBox;
using VRCOSC.App.UI.Views.Modules;
using VRCOSC.App.UI.Views.Packages;
using VRCOSC.App.UI.Views.Profiles;
using VRCOSC.App.UI.Views.Router;
using VRCOSC.App.UI.Views.Run;
using VRCOSC.App.SDK.OVR.Metadata;
using VRCOSC.App.UI.Views.Settings;
using Newtonsoft.Json;
using System.Threading.Tasks;
using VRCOSC.App.Actions;
using Semver;
using VRCOSC.App.ChatBox;
using VRCOSC.App.Profiles;
using VRCOSC.App.Router;
using Microsoft.UI.Xaml.Media.Animation;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VRCOSC_VIRA_Edits__Winui3_
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        #region Variables  

        public readonly PackagesView PackagesView;
        public readonly ModulesView ModulesView;
        public readonly RouterView RouterView;
        public readonly SettingsView SettingsView;
        public readonly ChatBoxView ChatBoxView;
        public readonly RunView RunView;
        public readonly AppDebugView AppDebugView;
        public readonly ProfilesView ProfilesView;
        public readonly AppSettingsView AppSettingsView;

        private readonly Storage storage = AppManager.GetInstance().Storage;

        public Observable<bool> ShowAppDebug { get; } = new();
        public Observable<bool> ShowRouter { get; } = new();
        public AppWindow m_AppWindow {  get; private set; }

        #endregion
        public MainWindow()
        {
            this.InitializeComponent();
            SettingsManager.GetInstance().Load();
            SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.EnableAppDebug).Subscribe(newValue => ShowAppDebug.Value = newValue, true);
            SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.EnableRouter).Subscribe(newValue => ShowRouter.Value = newValue, true);

            AppManager.GetInstance().Initialise();

            var installedUpdateChannel = SettingsManager.GetInstance().GetValue<UpdateChannel>(VRCOSCMetadata.InstalledUpdateChannel);

            copyOpenVrFiles();


            m_AppWindow = GetAppWindowForCurrentWindow();
            m_AppWindow.Title = "VIRA OSC Tool";

            Navigation.SelectionChanged += OnNavigationViewItemInvoked;

            Navigation.SelectedItem = Navigation.MenuItems[0];

            CustomizeTitleBar();

            Load();
        }

        private void OnNavigationViewItemInvoked(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            // Get the tag of the selected item
            var selectedItem = args.SelectedItem as NavigationViewItem;
            string selectedTag = selectedItem.Tag.ToString();



            // Navigate based on the selected tag
            switch (selectedTag)
            {
                case "home":
                    contentFrame.Navigate(typeof(homePage));
                    break;
                case "Settings":
                    // contentFrame.Navigate(typeof(settingsPage));
                    break;
                case "plugins":
                    // contentFrame.Navigate(typeof(modulesPage));
                    break;
                case "modules":
                    // contentFrame.Navigate(typeof(routingPage));
                    break;
            }
        }

        public AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }

        private bool SetTitleBarColors()
        {
            // Check to see if customization is supported.
            // The method returns true on Windows 10 since Windows App SDK 1.2,
            // and on all versions of Windows App SDK on Windows 11.
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                AppWindowTitleBar m_TitleBar = m_AppWindow.TitleBar;

                // Set active window colors.
                // Note: No effect when app is running on Windows 10
                // because color customization is not supported.
                m_TitleBar.ForegroundColor = Colors.White;
                m_TitleBar.BackgroundColor = Colors.Green;
                m_TitleBar.ButtonForegroundColor = Colors.White;
                m_TitleBar.ButtonBackgroundColor = Colors.SeaGreen;
                m_TitleBar.ButtonHoverForegroundColor = Colors.Gainsboro;
                m_TitleBar.ButtonHoverBackgroundColor = Colors.DarkSeaGreen;
                m_TitleBar.ButtonPressedForegroundColor = Colors.Gray;
                m_TitleBar.ButtonPressedBackgroundColor = Colors.LightGreen;

                // Set inactive window colors.
                // Note: No effect when app is running on Windows 10
                // because color customization is not supported.
                m_TitleBar.InactiveForegroundColor = Colors.Gainsboro;
                m_TitleBar.InactiveBackgroundColor = Colors.SeaGreen;
                m_TitleBar.ButtonInactiveForegroundColor = Colors.Gainsboro;
                m_TitleBar.ButtonInactiveBackgroundColor = Colors.SeaGreen;
                return true;
            }
            return false;
        }


        private void CustomizeTitleBar()
        {
            var appWindow = GetAppWindowForCurrentWindow();

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = appWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;

                // Set the title bar background to transparent so the acrylic effect can be applied.
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonForegroundColor = Colors.White;

                // Optionally set other properties like ButtonHoverBackgroundColor, etc.
            }
        }

        private void copyOpenVrFiles()
        {
            var runtimeOVRStorage = storage.GetStorageForDirectory("runtime/openvr");
            var runtimeOVRPath = runtimeOVRStorage.GetFullPath(string.Empty);

            var ovrFiles = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(file => file.Contains("OpenVR"));

            foreach (var file in ovrFiles)
            {
                File.WriteAllBytes(Path.Combine(runtimeOVRPath, getOriginalFileName(file)), getResourceBytes(file));
            }

            var manifest = new OVRManifest();
#if DEBUG
            manifest.Applications[0].BinaryPathWindows = Environment.ProcessPath!;
#else
        manifest.Applications[0].BinaryPathWindows = Path.Join(VelopackLocator.GetDefault(null).RootAppDir, "current", "VRCOSC.exe");
#endif
            manifest.Applications[0].ActionManifestPath = runtimeOVRStorage.GetFullPath("action_manifest.json");
            manifest.Applications[0].ImagePath = runtimeOVRStorage.GetFullPath("SteamImage.png");

            File.WriteAllText(Path.Join(runtimeOVRPath, "app.vrmanifest"), JsonConvert.SerializeObject(manifest, Formatting.Indented));
        }

        private static string getOriginalFileName(string fullResourceName)
        {
            var parts = fullResourceName.Split('.');
            return parts[^2] + "." + parts[^1];
        }

        private static byte[] getResourceBytes(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                throw new InvalidOperationException($"{resourceName} does not exist");
            }

            using var memoryStream = new MemoryStream();

            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        public async Task showLoadingOverlay(string title, ProgressAction progressAction)
        {
            // Show the overlay
            DispatcherQueue.TryEnqueue(() =>
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                LoadingTitle.Text = title;
                ProgressBar.IsIndeterminate = false; // False if specific progress updates will be shown
                ProgressBar.Value = 0; // Reset progress bar
            });
            

            // Hook up the completion event
            progressAction.OnComplete += async () =>
            {
                await hideLoadingOverlay();
            };
            
            // Track progress in a background task
            _ = Task.Run(async () =>
            {
                while (!progressAction.IsComplete)
                {
                    
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        LoadingDescription.Text = progressAction.Title;
                        ProgressBar.Value = progressAction.GetProgress();
                    });
                    
                    await Task.Delay(TimeSpan.FromSeconds(1d / 10d)); // Update every 100ms
                }

                // Final update when progress completes
                DispatcherQueue.TryEnqueue(() =>
                {
                    LoadingDescription.Text = "Finished!";
                    ProgressBar.Value = 1;
                });
            });

            // Execute the progress action
            await progressAction.Execute();            
        }

        public Task hideLoadingOverlay()
        {
            // Wrap the UI operation in a TaskCompletionSource for async compatibility
            var tcs = new TaskCompletionSource<bool>();

            DispatcherQueue.TryEnqueue(() =>
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                LoadingDescription.Text = string.Empty; // Clear lingering text
                ProgressBar.Value = 0; // Reset the progress bar
                tcs.SetResult(true); // Signal task completion
            });

            return tcs.Task;
        }

        private async void Load()
        {
            
            var loadingAction = new CompositeProgressAction();

            // Add loading actions
            // loadingAction.AddAction(PackageManager.GetInstance().Load());
            
            var installedVersion = SettingsManager.GetInstance().GetValue<string>(VRCOSCMetadata.InstalledVersion);

            if (!string.IsNullOrEmpty(installedVersion))
            {
                var installedVersionParsed = SemVersion.Parse(installedVersion, SemVersionStyles.Any);
                var currentVersion = SemVersion.Parse(AppManager.Version, SemVersionStyles.Any);

                // Refresh packages if we've upgraded or downgraded the version
                if (SemVersion.ComparePrecedence(installedVersionParsed, currentVersion) != 0)
                {
                    loadingAction.AddAction(PackageManager.GetInstance().RefreshAllSources(true));
                }
            }

            // Update installed version in settings
            SettingsManager.GetInstance().GetObservable<string>(VRCOSCMetadata.InstalledVersion).Value = AppManager.Version;

            // Add package update if auto-update is enabled
            if (SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.AutoUpdatePackages))
            {
                loadingAction.AddAction(new DynamicChildProgressAction(() => PackageManager.GetInstance().UpdateAllInstalledPackages()));
            }
            
            // Add other loading actions
            // loadingAction.AddAction(new DynamicProgressAction("Loading Profiles", () => ProfileManager.GetInstance().Load()));
            // loadingAction.AddAction(new DynamicProgressAction("Loading Modules", () => ModuleManager.GetInstance().LoadAllModules()));
            // loadingAction.AddAction(new DynamicProgressAction("Loading ChatBox", () => ChatBoxManager.GetInstance().Load()));
            // loadingAction.AddAction(new DynamicProgressAction("Loading Router", () => RouterManager.GetInstance().Load()));
            
            // Add first-time setup action
            if (!SettingsManager.GetInstance().GetValue<bool>(VRCOSCMetadata.FirstTimeSetupComplete))
            {
                loadingAction.AddAction(new DynamicChildProgressAction(() => PackageManager.GetInstance().InstallPackage(PackageManager.GetInstance().OfficialModulesSource)));
            }
            
            loadingAction.OnComplete += async () =>
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    SettingsManager.GetInstance().GetObservable<bool>(VRCOSCMetadata.FirstTimeSetupComplete).Value = true;
                    MainLayout.FadeInFromZero(500); // Fade in the main layout
                    AppManager.GetInstance().InitialLoadComplete(); // Await any async method called inside
                });
            };
            

            // Show the loading overlay
            await showLoadingOverlay("Welcome to VRCOSC", loadingAction);
            
        }
            

    }

    public static class UIElementExtensions
    {
        public static void FadeInFromZero(this UIElement element, double duration)
        {
            if (element == null) return;

            // Create a fade-in animation
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(duration))
            };

            // Apply the animation to the Opacity property
            var storyboard = new Storyboard();
            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Children.Add(animation);

            // Start the animation
            storyboard.Begin();
        }
    }

    public static class AppState
    {
        public static Dictionary<string, object> homePageState = new Dictionary<string, object>();
    }
}
