// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Graphics.Settings;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.Updater;

public abstract partial class VRCOSCUpdateManager : Component
{
    [Resolved]
    private VRCOSCGame game { get; set; } = null!;

    [Resolved]
    private NotificationContainer notifications { get; set; } = null!;

    [Resolved]
    private VRCOSCConfigManager configManager { get; set; } = null!;

    protected void PostUpdateAvailableNotification() => Schedule(() =>
    {
        notifications.Notify(new BasicNotification
        {
            Title = "Update Available",
            Description = "Click to install",
            Colour = ThemeManager.Current[ThemeAttribute.Pending],
            Icon = FontAwesome.Solid.ExclamationTriangle,
            ClickCallback = () => ApplyUpdatesAsync().ConfigureAwait(false)
        });
    });

    protected ProgressNotification PostProgressNotification()
    {
        var progressNotification = new ProgressNotification
        {
            Title = "Installing Update",
            Colour = ThemeManager.Current[ThemeAttribute.Pending],
            Icon = FontAwesome.Solid.Cog
        };

        Schedule(() => notifications.Notify(progressNotification));
        return progressNotification;
    }

    protected void PostSuccessNotification() => Schedule(() =>
    {
        notifications.Notify(new BasicNotification
        {
            Title = "Update Complete",
            Description = "Click to restart",
            Colour = ThemeManager.Current[ThemeAttribute.Success],
            Icon = FontAwesome.Solid.ExclamationTriangle,
            ClickCallback = () => PrepareUpdateAsync().ContinueWith(_ => Schedule(() => game.RequestExit()))
        });
    });

    protected void PostFailNotification() => Schedule(() =>
    {
        notifications.Notify(new BasicNotification
        {
            Title = "Update Failed",
            Description = "Report on the Discord server",
            Colour = ThemeManager.Current[ThemeAttribute.Failure],
            Icon = FontAwesome.Solid.ExclamationTriangle
        });
    });

    protected static void Log(string message) => Logger.Log(message, "updater");
    protected static void LogError(Exception e, string message = "") => Logger.Error(e, message, "updater", true);

    protected bool ApplyUpdatesImmediately => configManager.Get<UpdateMode>(VRCOSCSetting.UpdateMode) == UpdateMode.Auto;
    public abstract Task PerformUpdateCheck();
    protected abstract Task ApplyUpdatesAsync();
    protected abstract Task PrepareUpdateAsync();
}
