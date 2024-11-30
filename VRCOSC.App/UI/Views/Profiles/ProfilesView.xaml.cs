// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Controls;
using VRCOSC.App.Profiles;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Windows.Profiles;

namespace VRCOSC.App.UI.Views.Profiles;

public partial class ProfilesView
{
    private WindowManager windowManager = null!;

    public ProfilesView()
    {
        InitializeComponent();

        DataContext = ProfileManager.GetInstance();
    }

    private void ProfilesView_OnLoaded(object sender, RoutedEventArgs e)
    {
        windowManager = new WindowManager(this);
    }

    private void RemoveProfile_ButtonClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var profile = (Profile)button.Tag;

        var result = MessageBox.Show("Are you sure you want to delete this profile?\nDeleting will remove all saved module, persistence, and ChatBox data", "Uninstall Warning", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;

        ProfileManager.GetInstance().Profiles.Remove(profile);
    }

    private void EditProfile_ButtonClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var profile = (Profile)button.Tag;

        windowManager.TrySpawnChild(new ProfileEditWindow(profile));
    }

    private void AddProfile_ButtonClick(object sender, RoutedEventArgs e)
    {
        windowManager.TrySpawnChild(new ProfileEditWindow(null));
    }

    private void CopyProfile_ButtonClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var profile = (Profile)button.Tag;

        ProfileManager.GetInstance().CopyProfile(profile);
    }
}