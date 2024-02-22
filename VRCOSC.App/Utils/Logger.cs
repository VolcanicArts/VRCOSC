// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;

namespace VRCOSC.App.Utils;

public class Logger
{
    public static void Log(string message)
    {
        Console.WriteLine(message);
    }

    public static void Error(Exception e, string message)
    {
        Console.WriteLine(message + " - " + e.Message + " - " + e.StackTrace);
    }
}
