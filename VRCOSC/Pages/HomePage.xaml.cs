// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using Microsoft.Win32;

namespace VRCOSC.Pages;

public partial class HomePage
{
    public HomePage()
    {
        InitializeComponent();

        Title.Text = $"Welcome {getUserName()}!";

        AppManager.GetInstance().RegisterPage(PageLookup.Home, this);
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
}
