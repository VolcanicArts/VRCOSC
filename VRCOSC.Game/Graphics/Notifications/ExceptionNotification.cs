// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.Notifications;

public partial class ExceptionNotification : BasicNotification
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private Storage storage { get; set; } = null!;

    public ExceptionNotification(string message)
    {
        Title = "Exception Detected";
        Description = message + ".\nClick to open logs";
        Icon = FontAwesome.Solid.ExclamationTriangle;
        Colour = ThemeManager.Current[ThemeAttribute.Failure];
        ClickCallback += () => host.OpenFileExternally(storage.GetStorageForDirectory("logs").GetFullPath("runtime.log"));
    }
}
