// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows.Controls;
using System.Windows.Input;
using VRCOSC.App.UI.Core;

namespace VRCOSC.App.UI.Windows.Nodes;

public partial class PresetCreatorWindow : IManagedWindow
{
    public string PresetName { get; set; } = string.Empty;

    public PresetCreatorWindow()
    {
        InitializeComponent();
        DataContext = this;

        NameText.Focus();
    }

    public object GetComparer() => new();

    private void NameText_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(PresetName)) Close();
    }

    private void NameText_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        PresetName = NameText.Text;
    }
}