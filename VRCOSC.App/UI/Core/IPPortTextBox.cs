// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace VRCOSC.App.UI.Core;

public class IPPortTextBox : TextBox
{
    private static readonly Regex sanity_regex = new("[^0-9.:]+");
    private static readonly Regex is_ip_with_port_regex = new(@"\b(?:\d{1,3}\.){3}\d{1,3}(:\d{1,5})?\b");

    public IPPortTextBox()
    {
        MaxLength = 21;
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
            textBox.BorderBrush = !isIpAddressWithPort(text) ? Brushes.Red : Brushes.LightGray;
        }
    }

    private static bool isIpAddressWithPort(string text)
    {
        if (!is_ip_with_port_regex.IsMatch(text))
            return false;

        var parts = text.Split(':');

        if (parts.Length > 2)
            return false;

        if (!isIpAddress(parts[0]))
            return false;

        if (parts.Length == 2)
        {
            if (!int.TryParse(parts[1], out int port) || port < 0 || port > 65535)
                return false;
        }

        return true;
    }

    private static bool isIpAddress(string text)
    {
        var parts = text.Split('.');

        if (parts.Length != 4)
            return false;

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
