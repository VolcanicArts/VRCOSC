// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Input;

namespace VRCOSC.App.Modules;

public class Module
{
    public bool Enabled { get; set; }
    public string DisplayName { get; }
    public string Description { get; }

    public Module(string displayName, string description)
    {
        DisplayName = displayName;
        Description = description;
    }

    #region UI

    public ICommand UISettingsButton => new RelayCommand(_ => OnSettingsButtonClick());

    private void OnSettingsButtonClick()
    {
        MessageBox.Show("WOOOOOOOO");
    }

    #endregion
}
