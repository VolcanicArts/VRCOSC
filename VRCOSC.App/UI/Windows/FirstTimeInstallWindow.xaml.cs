// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Windows;

public partial class FirstTimeInstallWindow
{
    public FirstTimeInstallWindow()
    {
        InitializeComponent();
    }

    private void Discord_Click(object sender, RoutedEventArgs e)
    {
        new Uri("https://vrcosc.com/discord").OpenExternally();
    }
}