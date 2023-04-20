// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class DrawableVariable : Container
{
    private readonly ClipVariableMetadata clipVariable;

    public DrawableVariable(ClipVariableMetadata clipVariable)
    {
        this.clipVariable = clipVariable;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Masking = true;
        CornerRadius = 5;

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Light],
                RelativeSizeAxes = Axes.Both
            },
            new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(3),
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Width = 0.5f,
                        Padding = new MarginPadding(2),
                        Child = new SpriteText
                        {
                            Font = FrameworkFont.Regular.With(size: 20),
                            Text = clipVariable.Name + ":"
                        }
                    },
                    new LocalTextBox
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        RelativeSizeAxes = Axes.X,
                        Height = 25,
                        CornerRadius = 5,
                        Text = clipVariable.DisplayableFormat,
                        ReadOnly = true
                    }
                }
            }
        };
    }

    private partial class LocalTextBox : VRCOSCTextBox
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            BackgroundUnfocused = ThemeManager.Current[ThemeAttribute.Mid];
            BackgroundFocused = ThemeManager.Current[ThemeAttribute.Mid];
        }
    }
}
