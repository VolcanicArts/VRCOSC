// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace VRCOSC.App.Utils;

public static class ExceptionHandler
{
    public static bool SilenceWindow = false;

    private static bool isWindowShowing;

    public static void Handle(Exception e, string message = "", bool isCritical = false)
    {
        Logger.Error(e, message, LoggingTarget.Runtime, true);

        if (isWindowShowing || SilenceWindow) return;

        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(message))
        {
            sb.AppendLine(message);
            sb.AppendLine();
        }

        sb.AppendLine("Please report this on the Discord server or ask for help if you're developing a module");
        sb.AppendLine();
        sb.AppendLine("Press OK to join Discord server and report the error");
        sb.AppendLine("Press Cancel to ignore the error");
        sb.AppendLine();

        var proxyException = e;

        while (proxyException is not null)
        {
            sb.AppendLine(e.ToString());
            sb.AppendLine();
            sb.AppendLine(e.StackTrace);
            sb.AppendLine();
            proxyException = proxyException.InnerException;
        }

        isWindowShowing = true;

        var result = MessageBox.Show(sb.ToString(), $"VRCOSC has experienced a {(isCritical ? "critical" : "non-critical")} exception", MessageBoxButton.OKCancel, MessageBoxImage.Error);

        if (result == MessageBoxResult.OK)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = VRCOSCLinks.DISCORD_INVITE,
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory
            });
        }

        isWindowShowing = false;

        if (isCritical) Application.Current.Shutdown(-1);
    }

    public static void Handle(string message)
    {
        MessageBox.Show(message, "VRCOSC has experienced an error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
