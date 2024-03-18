// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VRCOSC.App.Profiles;
using VRCOSC.App.Utils;

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

        Profile.Name.Subscribe(globalProfileNameChanged);

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

    private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        var removeButton = (Button)sender;
        var itemToRemove = (Observable<string>)removeButton.Tag;
        Profile.LinkedAvatars.Remove(itemToRemove);
    }

    private void CancelEdit_ButtonClick(object sender, RoutedEventArgs e)
    {
        ProfileManager.GetInstance().ExitProfileEditWindow();
    }

    private void SaveEdit_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (!Profile.IsValidForSave()) return;

        if (OriginalProfile is null)
        {
            ProfileManager.GetInstance().Profiles.Add(Profile);
        }
        else
        {
            Profile.CopyTo(OriginalProfile);
        }

        ProfileManager.GetInstance().ExitProfileEditWindow();
    }

    private void AddLinkedAvatar_ButtonClick(object sender, RoutedEventArgs e)
    {
        Profile.LinkedAvatars.Add(new Observable<string>(string.Empty));
    }
}
