// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Input;

namespace VRCOSC.Modules;

public class Module
{
    public string DisplayName { get; }

    public Module(string displayName)
    {
        DisplayName = displayName;
    }

    #region UI

    public ICommand UISettingsButton => new RelayCommand(_ => OnSettingsButtonClick());

    private void OnSettingsButtonClick()
    {
        MessageBox.Show("WOOOOOOOO");
    }

    #endregion
}
