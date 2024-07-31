// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Controls;
using VRCOSC.App.Profiles;

namespace VRCOSC.App.UI.Views.Profiles;

public partial class ProfilesView
{
    public ProfilesView()
    {
        InitializeComponent();

        DataContext = ProfileManager.GetInstance();
    }

    private void RemoveProfile_ButtonClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var profile = (Profile)button.Tag;

        var result = MessageBox.Show("Are you sure you want to delete this profile?\nDeleting will remove all saved module, persistence, and ChatBox data", "Uninstall Warning", MessageBoxButton.YesNo);
        if (result != MessageBoxResult.Yes) return;

        ProfileManager.GetInstance().ExitProfileEditWindow();
        ProfileManager.GetInstance().Profiles.Remove(profile);
    }

    private void EditProfile_ButtonClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var profile = (Profile)button.Tag;

        ProfileManager.GetInstance().SpawnProfileEditWindow(profile);
    }

    private void AddProfile_ButtonClick(object sender, RoutedEventArgs e)
    {
        ProfileManager.GetInstance().SpawnProfileEditWindow();
    }
}
