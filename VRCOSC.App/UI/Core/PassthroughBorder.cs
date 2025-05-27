// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VRCOSC.App.UI.Core;

public class PassThroughBorder : Border
{
    /// <summary>
    /// Always return null so that WPF never considers the border itself a hit-test target.
    /// Children live in their own visual layer and will still be hit-tested!
    /// </summary>
    protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
    {
        return null!;
    }

    // (Optional) If you want absolutely no clipping interference:
    protected override Geometry GetLayoutClip(Size layoutSlotSize)
    {
        return null!;
    }
}