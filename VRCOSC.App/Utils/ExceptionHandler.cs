﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
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

        sb.AppendLine("-------------------------------------------------------------");
        sb.AppendLine($"Version: {AppManager.Version}");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(message))
        {
            sb.AppendLine(message);
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(e.Message))
        {
            sb.AppendLine(e.Message);
            sb.AppendLine();
        }

        sb.AppendLine(isCritical ? "Please report this on the Discord server" : "Please report this on the Discord server or ask for help if you're developing a module");
        sb.AppendLine();
        sb.AppendLine("Press OK to join the Discord server");
        sb.AppendLine("Press Cancel to ignore the error");
        sb.AppendLine("-------------------------------------------------------------");
        sb.AppendLine();

        var proxyException = e;

        while (proxyException is not null)
        {
            sb.AppendLine(e.ToString());
            sb.AppendLine();
            proxyException = proxyException.InnerException;
        }

        isWindowShowing = true;

        var result = MessageBox.Show(sb.ToString(), $"VRCOSC has experienced a {(isCritical ? "critical" : "non-critical")} exception", MessageBoxButton.OKCancel, isCritical ? MessageBoxImage.Error : MessageBoxImage.Warning);

        if (result == MessageBoxResult.OK)
        {
            VRCOSCLinks.DISCORD_INVITE.OpenExternally();
        }

        isWindowShowing = false;

        if (isCritical) Application.Current.Shutdown(-1);
    }

    public static void Handle(string message)
    {
        isWindowShowing = true;
        MessageBox.Show(message, "VRCOSC has experienced an error", MessageBoxButton.OK, MessageBoxImage.Error);
        isWindowShowing = false;
    }
}