// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.Notifications;

public partial class PortInUseNotification : BasicNotification
{
    public PortInUseNotification(int port)
    {
        Title = "OSC Error";
        Description = $"OSC port {port} is already in use by another program";
        Icon = FontAwesome.Solid.ExclamationTriangle;
        Colour = ThemeManager.Current[ThemeAttribute.Failure];
    }

    public PortInUseNotification(string message)
    {
        Title = "OSC Error";
        Description = message;
        Icon = FontAwesome.Solid.ExclamationTriangle;
        Colour = ThemeManager.Current[ThemeAttribute.Failure];
    }
}
