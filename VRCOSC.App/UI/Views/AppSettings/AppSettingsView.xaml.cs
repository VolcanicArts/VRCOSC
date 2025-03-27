// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Controls.Primitives;

// ReSharper disable UnusedMember.Global

namespace VRCOSC.App.UI.Views.AppSettings;

public partial class AppSettingsView
{
    public AppSettingsView()
    {
        InitializeComponent();
        DataContext = this;
        setTab(0);
    }

    public void FocusBehaviourTab()
    {
        BehaviourTabButton.IsChecked = true;
        BehaviourTabButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
    }

    private void setTab(int tab)
    {
        GeneralContainer.Visibility = tab == 0 ? Visibility.Visible : Visibility.Collapsed;
        BehaviourContainer.Visibility = tab == 1 ? Visibility.Visible : Visibility.Collapsed;
        SpeechContainer.Visibility = tab == 2 ? Visibility.Visible : Visibility.Collapsed;
        SteamVRContainer.Visibility = tab == 3 ? Visibility.Visible : Visibility.Collapsed;
        RouterContainer.Visibility = tab == 4 ? Visibility.Visible : Visibility.Collapsed;
        StartupContainer.Visibility = tab == 5 ? Visibility.Visible : Visibility.Collapsed;
        DebugContainer.Visibility = tab == 6 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void GeneralTabButton_OnClick(object sender, RoutedEventArgs e) => setTab(0);
    private void BehaviourTabButton_OnClick(object sender, RoutedEventArgs e) => setTab(1);
    private void SpeechTabButton_OnClick(object sender, RoutedEventArgs e) => setTab(2);
    private void SteamVRTabButton_OnClick(object sender, RoutedEventArgs e) => setTab(3);
    private void RouterTabButton_OnClick(object sender, RoutedEventArgs e) => setTab(4);
    private void StartupTabButton_OnClick(object sender, RoutedEventArgs e) => setTab(5);
    private void DebugTabButton_OnClick(object sender, RoutedEventArgs e) => setTab(6);
}