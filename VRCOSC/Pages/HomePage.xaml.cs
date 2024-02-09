// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using Microsoft.Win32;
using VRCOSC.Windows;

namespace VRCOSC.Pages;

public partial class HomePage
{
    public HomePage()
    {
        InitializeComponent();

        Title.Text = $"Welcome {getUserName()}!";
    }

    private static string getUserName()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Office\Common\UserInfo");
            if (key is null) return Environment.UserName;

            var userNameValue = key.GetValue("UserName");
            return userNameValue is not null ? userNameValue.ToString()! : Environment.UserName;
        }
        catch (Exception)
        {
            return Environment.UserName;
        }
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        // PopupContent.Opacity = 0;
        //
        // // Open the popup
        // Popup.IsOpen = true;
        //
        // // Create a DoubleAnimation to animate opacity
        // DoubleAnimation animation = new DoubleAnimation
        // {
        //     To = 1, // Final opacity value
        //     Duration = TimeSpan.FromSeconds(1), // Animation duration
        //     EasingFunction = new QuarticEase() // Quartic easing
        // };
        //
        // // Set the target property to animate
        // PopupContent.BeginAnimation(OpacityProperty, animation);

        var window = new TestWindow();
        window.Show();
    }
}
