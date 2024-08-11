// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using VRCOSC.App.Utils;

namespace VRCOSC.App.UI.Core;

public partial class KeybindPicker : INotifyPropertyChanged
{
    public ModifierKeys ModifierKeys;
    public Key Key;

    private string stringRepresentation = string.Empty;

    public string StringRepresentation
    {
        get => stringRepresentation;
        set
        {
            stringRepresentation = value;
            OnPropertyChanged();
        }
    }

    public KeybindPicker()
    {
        InitializeComponent();

        DataContext = this;
    }

    private void KeybindPicker_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        Focus();
        e.Handled = true;
    }

    private void KeybindPicker_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl &&
            e.Key != Key.LeftShift && e.Key != Key.RightShift &&
            e.Key != Key.LeftAlt && e.Key != Key.RightAlt &&
            e.Key != Key.LWin && e.Key != Key.RWin)
        {
            var key = e.Key;

            var modifiersString = string.Empty;

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                modifiersString += "Ctrl + ";

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                modifiersString += "Shift + ";

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                modifiersString += "Alt + ";

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Windows))
                modifiersString += "Win + ";

            ModifierKeys = Keyboard.Modifiers;
            Key = key;
            StringRepresentation = modifiersString + key.ToReadableString();

            e.Handled = true;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
