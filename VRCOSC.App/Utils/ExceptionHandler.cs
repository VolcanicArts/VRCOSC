// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Text;
using System.Windows;

namespace VRCOSC.App.Utils;

public static class ExceptionHandler
{
    public static void Handle(Exception e, string message = "")
    {
        Logger.Error(e, message);

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
        sb.AppendLine(e.GetType().FullName);
        sb.AppendLine(e.StackTrace);

        MessageBox.Show(sb.ToString(), "VRCOSC has experienced an exception", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
