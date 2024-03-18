// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace VRCOSC.App.Utils;

public static class ImageLoader
{
    private static readonly Dictionary<string, BitmapImage> cache = new();

    public static void RetrieveFromURL(string url, Action<BitmapImage, bool> onLoaded)
    {
        if (cache.TryGetValue(url, out var cachedBitmapImage))
        {
            onLoaded.Invoke(cachedBitmapImage, true);
            return;
        }

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.UriSource = new Uri(url, UriKind.Absolute);
        bitmap.EndInit();

        bitmap.DownloadCompleted += (_, _) => onLoaded.Invoke(bitmap, false);
        cache.Add(url, bitmap);
    }
}
