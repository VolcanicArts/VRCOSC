// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Reflection;

namespace VRCOSC.App.Utils;

public static class ResourceHelper
{
    public static string GetOriginalFileName(string fullResourceName)
    {
        var parts = fullResourceName.Split('.');
        return parts[^2] + "." + parts[^1];
    }

    public static byte[] GetResourceBytes(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
            throw new InvalidOperationException($"{resourceName} does not exist");

        using var memoryStream = new MemoryStream();

        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}