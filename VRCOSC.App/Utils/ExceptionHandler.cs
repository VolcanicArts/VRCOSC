// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace VRCOSC.App.Utils;

public static class ExceptionHandler
{
    private static bool isWindowShowing;

    public static void Handle(Exception e, string message = "", bool isCritical = false)
    {
        Logger.Error(e, message, LoggingTarget.Runtime, true);

        if (isWindowShowing) return;

        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(message))
        {
            sb.AppendLine(message);
            sb.AppendLine();
        }

        sb.AppendLine(e.Message);
        sb.AppendLine();
        sb.AppendLine("Please report this on the Discord server or ask for help if you're developing a module");
        sb.AppendLine();
        sb.AppendLine("Press OK to join Discord server and report the issue");
        sb.AppendLine("Press cancel to ignore the error");
        sb.AppendLine();
        sb.AppendLine(e.GetType().FullName);
        sb.AppendLine(e.StackTrace);

        var innerException = e.InnerException;

        while (innerException is not null)
        {
            sb.AppendLine();
            sb.AppendLine(e.GetType().FullName);
            sb.AppendLine(innerException.StackTrace);
            innerException = innerException.InnerException;
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
        Logger.Log($"Message: {message}");
        MessageBox.Show(message, "VRCOSC has experienced an error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
