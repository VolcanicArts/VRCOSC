// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Pages.Run;

public partial class MiscView
{
    private readonly Typeface measuringTypeface = new(new FontFamily("Liberation Mono"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

    public MiscView()
    {
        InitializeComponent();

        AppManager.GetInstance().State.Subscribe(onAppManagerStateChange, true);
        AppManager.GetInstance().VRChatOscClient.OnParameterSent += OnParameterSent;
    }

    private void onAppManagerStateChange(AppManagerState newState) => Dispatcher.Invoke(() =>
    {
        switch (newState)
        {
            case AppManagerState.Starting:
                ChatBoxText.Text = "";
                break;
        }
    });

    private void OnParameterSent(VRChatOscMessage message) => Dispatcher.Invoke(() =>
    {
        if (!message.IsChatboxInput) return;

        var originalText = (string)message.ParameterValue;
        var lines = originalText.Split('\n');

        var formattedLines = new List<string>();
        lines.ForEach(line =>
        {
            var formattedLine = FormatText(line, 185);
            formattedLines.Add(formattedLine.Trim());
        });

        var finalText = string.Join("\n", formattedLines);

        ChatBoxText.Text = finalText;
    });

    public string FormatText(string text, double maxWidth)
    {
        var formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, measuringTypeface, 12, Brushes.Black, 1.0);
        var totalWidth = formattedText.Width;

        while (totalWidth > maxWidth)
        {
            var closestSpaceIndex = findClosestSpaceBeforeWidth(text, totalWidth, maxWidth);

            // TODO: If no space, split on max width index

            if (closestSpaceIndex == -1)
            {
                break;
            }

            text = text.Substring(0, closestSpaceIndex) + Environment.NewLine + text.Substring(closestSpaceIndex + 1);

            formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, measuringTypeface, 12, Brushes.Black, 1.0);
            totalWidth = formattedText.Width;
        }

        return text;
    }

    private int findClosestSpaceBeforeWidth(string text, double totalWidth, double maxWidth)
    {
        var closestSpaceIndex = -1;
        var currentWidth = totalWidth;

        for (var i = text.Length - 1; i >= 0; i--)
        {
            if (text[i] == ' ')
            {
                var formattedText = new FormattedText(text.Substring(0, i), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, measuringTypeface, 12, Brushes.Black, 1.0);
                currentWidth = formattedText.Width;

                if (currentWidth <= maxWidth)
                {
                    closestSpaceIndex = i;
                    break;
                }
            }
        }

        return closestSpaceIndex;
    }
}
