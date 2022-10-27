// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;

namespace VRCOSC.Game.Graphics.Updater;

public abstract class VRCOSCUpdateManager : Container
{
    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private NotificationContainer notifications { get; set; } = null!;

    protected void PostCheckNotification() => Schedule(() =>
    {
        notifications.Notify(new BasicNotification
        {
            Title = "Update Available",
            Description = "Click to install",
            Colour = VRCOSCColour.Green,
            Icon = FontAwesome.Solid.ExclamationTriangle,
            ClickCallback = () => ApplyUpdates()
        });
    });

    protected ProgressNotification PostProgressNotification()
    {
        var progressNotification = new ProgressNotification
        {
            Title = "Installing Update",
            Colour = VRCOSCColour.Yellow,
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
            Colour = VRCOSCColour.Green,
            Icon = FontAwesome.Solid.ExclamationTriangle,
            ClickCallback = RequestRestart
        });
    });

    protected void PostFailNotification() => Schedule(() =>
    {
        notifications.Notify(new BasicNotification
        {
            Title = "Update Failed",
            Description = "Click to reinstall",
            Colour = VRCOSCColour.Red,
            Icon = FontAwesome.Solid.ExclamationTriangle,
            ClickCallback = () => host.OpenUrlExternally("https://github.com/VolcanicArts/VRCOSC/releases/latest")
        });
    });

    public abstract Task CheckForUpdate(string repo, bool useDelta = true);
    protected abstract Task ApplyUpdates();
    protected abstract void RequestRestart();
}
