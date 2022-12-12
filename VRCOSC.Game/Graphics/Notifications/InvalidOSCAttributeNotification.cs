// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Design;

public partial class InvalidOSCAttributeNotification : BasicNotification
{
    public InvalidOSCAttributeNotification(string attributeName)
    {
        Title = "OSC Error";
        Description = $"Invalid OSC {attributeName} detected. Please check your settings";
        Icon = FontAwesome.Solid.ExclamationTriangle;
        Colour = ThemeManager.Current[ThemeAttribute.Failure];
    }
}
