// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Views.Information;

public partial class InformationView
{
    public InformationView()
    {
        InitializeComponent();
    }

    private void Discord_Click(object sender, RoutedEventArgs e) => new Uri("https://vrcosc.com/discord").OpenExternally();
    private void GitHub_Click(object sender, RoutedEventArgs e) => new Uri("https://github.com/VolcanicArts/VRCOSC").OpenExternally();
    private void KoFi_Click(object sender, RoutedEventArgs e) => new Uri("https://ko-fi.com/volcanicarts").OpenExternally();
}