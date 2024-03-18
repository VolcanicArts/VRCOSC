// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VRCOSC.App.Profiles;

namespace VRCOSC.App.Pages.Profiles;

public partial class ProfilesPage
{
    public ProfilesPage()
    {
        InitializeComponent();

        DataContext = ProfileManager.GetInstance();
    }

    private void RemoveProfile_ButtonClick(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var profile = (Profile)button.Tag;

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

public class BackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var index = System.Convert.ToInt32(value);
        return index % 2 == 0 ? Application.Current.Resources["CBackground3"] : Application.Current.Resources["CBackground4"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
