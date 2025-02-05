// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VRCOSC.App.UI.Core;

public partial class ChatBoxTextBox
{
    public ChatBoxTextBox()
    {
        InitializeComponent();
    }

    private const int max_lines = 9;
    private static readonly char[] separator = new[] { '\n' };

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var textBox = (sender as TextBox)!;
            var selectionStart = textBox.SelectionLength == 0 ? textBox.CaretIndex : textBox.SelectionStart;

            if (textBox.Text.Split("\n").Length < max_lines)
            {
                textBox.Text = textBox.Text.Insert(selectionStart, "\n");
                textBox.SelectionStart = selectionStart + "\n".Length;
                textBox.SelectionLength = 0;
            }

            e.Handled = true;
        }
    }

    private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(DataFormats.UnicodeText)) return;

        var textBox = (sender as TextBox)!;
        var selectionStart = textBox.SelectionStart;
        var selectionLength = textBox.SelectionLength;

        var pastedText = e.DataObject.GetData(DataFormats.UnicodeText) as string ?? string.Empty;

        pastedText = pastedText.Replace(Environment.NewLine, "\n");

        var newlineCount = pastedText.Split(separator, StringSplitOptions.None).Length - 1;
        var currentLineCount = textBox.LineCount;

        var selectedText = textBox.Text.Substring(selectionStart, selectionLength);
        var selectedLineCount = selectedText.Split(separator, StringSplitOptions.None).Length;

        var remainingLines = Math.Max(max_lines - (currentLineCount - selectedLineCount), 0);
        var linesToAdd = Math.Min(remainingLines, newlineCount + 1);
        var lines = pastedText.Split(separator, StringSplitOptions.None);

        var newTextToAdd = string.Join("\n", lines.Take(linesToAdd));
        var newText = textBox.Text.Remove(selectionStart, selectionLength).Insert(selectionStart, newTextToAdd);

        textBox.Text = newText;

        textBox.SelectionStart = selectionStart + newTextToAdd.Length;
        textBox.SelectionLength = 0;

        e.CancelCommand();

        textBox.Text = trimLinesToMax(textBox.Text);
    }

    private void TextBox_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var textBox = (TextBox)sender;

        // I hate this, but it forces horizontal text position to be recalculated
        textBox.TextAlignment = TextAlignment.Left;
        textBox.TextAlignment = TextAlignment.Center;
    }

    private string trimLinesToMax(string text)
    {
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

        var trimmedLines = lines.Take(max_lines);

        if (trimmedLines.Count() == max_lines && trimmedLines.Last().EndsWith('\n'))
        {
            trimmedLines = trimmedLines.Take(max_lines - 1).Concat(new[] { trimmedLines.Last().TrimEnd('\n') });
        }

        return string.Join("\n", trimmedLines);
    }
}