// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VRCOSC.App.Profiles;

namespace VRCOSC.App.Pages.Profiles;

public partial class ProfileEditWindow
{
    public Profile? OriginalProfile { get; }
    public Profile Profile { get; }
    public bool ForceClose { get; set; }

    public ProfileEditWindow(Profile? profile)
    {
        InitializeComponent();

        OriginalProfile = profile;
        Profile = OriginalProfile?.Clone() ?? new Profile();

        Profile.Name.Subscribe(globalProfileNameChanged, true);

        if (OriginalProfile is not null)
        {
            Profile.Name.Subscribe(profileNameChanged, true);
            TitleText.Text = "Edit Profile";
        }
        else
        {
            Title = "Creating New Profile";
            TitleText.Text = "Create Profile";
        }

        DataContext = Profile;
    }

    private void globalProfileNameChanged(string _)
    {
        NameTextBox.BorderBrush = !Profile.IsValidForSave() ? Brushes.Red : Brushes.Transparent;
    }

    private void profileNameChanged(string newName)
    {
        Title = $"Editing {newName} Profile";
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        Profile.Name.Unsubscribe(globalProfileNameChanged);
        Profile.Name.Unsubscribe(profileNameChanged);
    }

    private void ProfileEditWindow_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        var focusedElement = FocusManager.GetFocusedElement(this) as FrameworkElement;

        if (e.OriginalSource is not TextBox && focusedElement is TextBox)
        {
            Keyboard.ClearFocus();
        }
    }

    private void ProfileEditWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        if (!Profile.IsValidForSave() && !ForceClose)
        {
            e.Cancel = true;
        }
    }
}
