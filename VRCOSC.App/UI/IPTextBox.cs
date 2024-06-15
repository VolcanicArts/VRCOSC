// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VRCOSC.App.UI;

public class IPTextBox : TextBox
{
    private static readonly Regex sanity_regex = new("[^0-9.]+");
    private static readonly Regex is_ip_regex = new(@"\b(?:\d{1,3}\.){3}\d{1,3}\b");

    public IPTextBox()
    {
        MaxLength = 15;
        PreviewTextInput += OnPreviewTextInput;
        TextChanged += OnTextChanged;
        DataObject.AddPastingHandler(this, OnPaste);
    }

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !isTextAllowed(e.Text);
    }

    private static bool isTextAllowed(string text) => !sanity_regex.IsMatch(text);

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            var text = textBox.Text;
            textBox.BorderBrush = !isIpAddress(text) ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.White;
        }
    }

    private static bool isIpAddress(string text)
    {
        if (!is_ip_regex.IsMatch(text))
            return false;

        var parts = text.Split('.');

        foreach (var part in parts)
        {
            if (int.TryParse(part, out int num))
            {
                if (num < 0 || num > 255)
                    return false;
            }
            else
                return false;
        }

        return true;
    }

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(DataFormats.Text))
        {
            if (e.DataObject.GetData(DataFormats.Text) is not string text) return;

            if (!isTextAllowed(text))
            {
                e.CancelCommand();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }
}
