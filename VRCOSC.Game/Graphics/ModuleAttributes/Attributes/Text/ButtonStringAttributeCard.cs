// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleAttributes.Attributes.Text;

public sealed partial class ButtonStringAttributeCard : TextAttributeCard<StringTextBox, string>
{
    private readonly ModuleAttributeWithButton attributeWithButton;

    public ButtonStringAttributeCard(ModuleAttributeWithButton attributeData)
        : base(attributeData)
    {
        attributeWithButton = attributeData;
    }

    protected override Drawable CreateContent()
    {
        return new GridContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 30,
            ColumnDimensions = new[]
            {
                new Dimension(GridSizeMode.Relative, 0.75f),
                new Dimension(GridSizeMode.Absolute, 5),
                new Dimension()
            },
            Content = new[]
            {
                new[]
                {
                    base.CreateContent(),
                    null,
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(5),
                        Child = new TextButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Text = attributeWithButton.ButtonText,
                            Masking = true,
                            CornerRadius = 5,
                            Action = attributeWithButton.ButtonAction,
                            BackgroundColour = ThemeManager.Current[ThemeAttribute.Action]
                        }
                    }
                }
            }
        };
    }
}
