// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VRCOSC.App.Profiles;
using VRCOSC.App.UI.Core;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Windows.Profiles;

public partial class ProfileEditWindow : IManagedWindow
{
    public Profile? OriginalProfile { get; }
    public Profile Profile { get; }

    public ProfileEditWindow(Profile? profile)
    {
        InitializeComponent();

        OriginalProfile = profile;
        Profile = OriginalProfile?.Clone(true) ?? new Profile();

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

    private bool isValidForSave() => !string.IsNullOrEmpty(Profile.Name.Value);

    private void globalProfileNameChanged()
    {
        NameTextBox.BorderBrush = isValidForSave() ? Brushes.Transparent : Brushes.Red;
    }

    private void profileNameChanged(string? newName)
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

    private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
    {
        var removeButton = (Button)sender;
        var itemToRemove = (Observable<string>)removeButton.Tag;
        Profile.LinkedAvatars.Remove(itemToRemove);
    }

    private void SaveEdit_ButtonClick(object sender, RoutedEventArgs e)
    {
        if (!isValidForSave()) return;

        if (OriginalProfile is null)
        {
            ProfileManager.GetInstance().Profiles.Add(Profile);
        }
        else
        {
            Profile.CopyTo(OriginalProfile);
        }

        Close();
    }

    private void AddLinkedAvatar_ButtonClick(object sender, RoutedEventArgs e)
    {
        Profile.LinkedAvatars.Add(new Observable<string>(string.Empty));
    }

    // generate a new object if there's no original profile to let the user have multiple new profile windows open
    public object GetComparer() => OriginalProfile ?? new object();
}