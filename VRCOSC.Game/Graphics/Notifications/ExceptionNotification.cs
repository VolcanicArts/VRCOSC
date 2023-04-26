// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.Notifications;

public partial class ExceptionNotification : BasicNotification
{
    public ExceptionNotification(string message)
    {
        Title = "Exception Detected";
        Description = message;
        Icon = FontAwesome.Solid.ExclamationTriangle;
        Colour = ThemeManager.Current[ThemeAttribute.Failure];
    }
}
